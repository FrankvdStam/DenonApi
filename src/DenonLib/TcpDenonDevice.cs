using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DenonLib
{
    public class TcpDenonDevice : IDenonDevice, IDisposable
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _networkStream;

        public TcpDenonDevice(string ip, int port)
        {
            _tcpClient = new TcpClient(ip, port);
            _networkStream = _tcpClient.GetStream();
            _networkStream.ReadTimeout = 200;
        }

        #region TCP
        /// <summary>
        /// Sends the given message over tcp after appending a carriage return (0x0D) to it, attempts to read the response into the out response string. Returns true if a response was given.
        /// </summary>
        private void SendMessage(string message)
        {
            //Send command
            var bytes = Encoding.ASCII.GetBytes(message + (char)0x0D);
            _networkStream.Write(bytes, 0, bytes.Length);
        }

        private bool SendMessage(string message, out string response)
        {
            response = "";

            SendMessage(message);

            //Get response
            try
            {
                response = GetResponse();
                return true;
            }
            catch
            {
                return false;
            }
        }

        //TODO: find better solution then Thread.Sleep. For now it works better than ReadTimeout.
        private string GetResponse()
        {
            Thread.Sleep(200);
            MemoryStream stream = new MemoryStream();
            while (_networkStream.DataAvailable)
            {
                var buffer = new byte[2048];
                int respLength = _networkStream.Read(buffer, 0, buffer.Length);
                stream.Write(buffer, 0, buffer.Length);
                Thread.Sleep(50);
            }
            return System.Text.Encoding.ASCII.GetString(stream.GetBuffer()).Replace("\0", string.Empty);
        }
        #endregion

        #region Power
        public void PowerOn()
        {
            SendMessage("PWON");
        }

        public void PowerStandby()
        {
            SendMessage("PWSTANDBY");
        }
        #endregion

        #region Master Volume  ========================================================================================================
        public decimal GetMasterVolume()
        {
            SendMessage("MV?", out string response);
            return VolumeStringToDecimal(response);
        }

        public void SetMasterVolume(decimal volume)
        {
            if (volume >= 0 && volume <= 98)
            {
                SendMessage("MV"  + DecimalToVolumeString(volume));
            }
            else
            {
                throw new ArgumentException($"Volume must be between 0 and 98, actual is {volume}");
            }
        }

        public void MasterVolumeUp()
        {
            SendMessage("MVUP", out _);
        }

        public void MasterVolumeDown()
        {
            SendMessage("MVDOWN", out _);
        }
        #endregion

        #region Channel Volume ========================================================================================================
        public void ChannelVolumeUp(Channel channel)
        {
            SendMessage("CV" + ChannelToStringLookup[channel] + " UP");
        }

        public void ChannelVolumeDown(Channel channel)
        {
            SendMessage("CV" + ChannelToStringLookup[channel] + " DOWN");
        }

        public void SetChannelVolume(Channel channel, decimal volume)
        {
            if (volume >= 38 && volume <= 62)
            {
                SendMessage("CV" + ChannelToStringLookup[channel] + " " + DecimalToVolumeString(volume));
            }
            else
            {
                throw new ArgumentException($"Volume must be between 0 and 98, actual is {volume}");
            }

            
        }

        public Dictionary<Channel, decimal> GetChannelStatus()
        {
            Dictionary<Channel, decimal> channelVolumes = new Dictionary<Channel, decimal>();
            SendMessage("CV?", out string result);
            var lines = result.Replace("CV", string.Empty).Split('\r');
            foreach (var s in lines)
            {
                var split = s.Split(' ');
                if (split[0] == "END")
                {
                    break;
                }

                if (!StringToChannelLookup.ContainsKey(split[0]))
                {
                    continue;
                }

                Channel c = StringToChannelLookup[split[0]];
                channelVolumes[c] = VolumeStringToDecimal(split[1]);
            }
            return channelVolumes;
        }

        public void ResetChannels()
        {
            SendMessage("ZRL");
        }
        #endregion

        #region Mute ========================================================================================================
        public void Mute()
        {
            SendMessage("MUON");
        }

        public void UnMute()
        {
            SendMessage("MUOFF");
        }

        public bool IsMute()
        {
            SendMessage("MU?", out string result);
            return result.StartsWith("MUON");
        }
        #endregion

        #region Input source ========================================================================================================
        public void SelectInputSource(InputSource s)
        {
            SendMessage("SI" + InputSourceToStringLookup[s]);
        }

        public void GetSelectedInputSourceStatus()
        {
            SendMessage("SI?", out string result);
        }
        #endregion

        #region Util ========================================================================================================
        /// <summary>
        /// Convert a decimal to a string format that denon devices understand
        /// </summary>
        private static string DecimalToVolumeString(decimal d)
        {
            string result = "";
            //Round to nearest half
            d = Math.Round(d * 2, MidpointRounding.AwayFromZero) / 2;

            //Start with a zero if the value has only 1 decimal
            if (d < 10)
            {
                result += "0";
            }
            result += d.ToString();
            result = result.Replace(".", string.Empty);
            return result;
        }

        // example formats: CVFL 50\r -> 50 / CVFL 745\r -> 74.5
        /// <summary>
        /// Cleans and parses a device volume string to decimal
        /// </summary>
        private static decimal VolumeStringToDecimal(string data)
        {
            int? index = null;
            //Cleanup: Get first index of decimal
            for (int i = 0; i < data.Length; i++)
            {
                if (char.IsDigit(data[i]))
                {
                    index = i;
                    break;
                }
            }

            if (index == null)
            {
                throw new Exception($"Couldn't parse volume returned from the device: {data}");
            }

            data = data.Substring(index.Value, data.Length - index.Value);

            //Check if there are 3 decimals to parse
            if (data.Length > 3)
            {

            }

            if (!decimal.TryParse(data.Substring(0, 2), out decimal result))
            {
                throw new Exception($"Couldn't parse volume returned from the device: {data}");
            }

            //Find the 3rd possible digit, containing a halfstep
            if (data.Length >= 3 && char.IsDigit(data[2]))
            {
                result += 0.5M;
            }
            return result;
        }
        #endregion

        #region Lookup
        private static readonly Dictionary<Channel, string> ChannelToStringLookup = new Dictionary<Channel, string>()
        {
            { Channel.FrontLeft             , "FL"  },
            { Channel.FrontRight            , "FR"  },
            { Channel.Center                , "C"   },
            { Channel.SubWoofer1            , "SW"  },
            { Channel.SubWoofer2            , "SW2" },
            { Channel.SurroundLeft          , "SL"  },
            { Channel.SurroundRight         , "SR"  },
            { Channel.SurroundBackLeft      , "SBL" },
            { Channel.SurroundBackRight     , "SBR" },
            { Channel.SurroundBack          , "SB"  },
            { Channel.FrontHeightLeft       , "FHL" },
            { Channel.FrontHeightRight      , "FHR" },
            { Channel.FrontWideLeft         , "FWL" },
            { Channel.FrontWideRight        , "FWR" },
            { Channel.TopFrontLeft          , "TFL" },
            { Channel.TopFrontRight         , "TFR" },
            { Channel.TopMiddleLeft         , "TML" },
            { Channel.TopMiddleRight        , "TMR" },
            { Channel.TopRearLeft           , "TRL" },
            { Channel.TopRearRight          , "TRR" },
            { Channel.RearHeightLeft        , "RHL" },
            { Channel.RearHeightRight       , "RHR" },
            { Channel.FrontDolbyLeft        , "FDL" },
            { Channel.FrontDolbyRight       , "FDR" },
            { Channel.SurroundDolbyLeft     , "SDL" },
            { Channel.SurroundDolbyRight    , "SDR" },
            { Channel.BackDolbyLeft         , "BDL" },
            { Channel.BackDolbyRight        , "BDR" },
            { Channel.SurroundHeightLeft    , "SHL" },
            { Channel.SurroundHeightRight   , "SHR" },
            { Channel.TopSurround           , "TS"  },
        };

        private static readonly Dictionary<string, Channel> StringToChannelLookup = new Dictionary<string, Channel>()
        {
            { "FL"  , Channel.FrontLeft           },
            { "FR"  , Channel.FrontRight          },
            { "C"   , Channel.Center              },
            { "SW"  , Channel.SubWoofer1          },
            { "SW2" , Channel.SubWoofer2          },
            { "SL"  , Channel.SurroundLeft        },
            { "SR"  , Channel.SurroundRight       },
            { "SBL" , Channel.SurroundBackLeft    },
            { "SBR" , Channel.SurroundBackRight   },
            { "SB"  , Channel.SurroundBack        },
            { "FHL" , Channel.FrontHeightLeft     },
            { "FHR" , Channel.FrontHeightRight    },
            { "FWL" , Channel.FrontWideLeft       },
            { "FWR" , Channel.FrontWideRight      },
            { "TFL" , Channel.TopFrontLeft        },
            { "TFR" , Channel.TopFrontRight       },
            { "TML" , Channel.TopMiddleLeft       },
            { "TMR" , Channel.TopMiddleRight      },
            { "TRL" , Channel.TopRearLeft         },
            { "TRR" , Channel.TopRearRight        },
            { "RHL" , Channel.RearHeightLeft      },
            { "RHR" , Channel.RearHeightRight     },
            { "FDL" , Channel.FrontDolbyLeft      },
            { "FDR" , Channel.FrontDolbyRight     },
            { "SDL" , Channel.SurroundDolbyLeft   },
            { "SDR" , Channel.SurroundDolbyRight  },
            { "BDL" , Channel.BackDolbyLeft       },
            { "BDR" , Channel.BackDolbyRight      },
            { "SHL" , Channel.SurroundHeightLeft  },
            { "SHR" , Channel.SurroundHeightRight },
            { "TS"  , Channel.TopSurround         },
        };

        private static readonly Dictionary<InputSource, string> InputSourceToStringLookup = new Dictionary<InputSource, string>()
        {
            { InputSource.Phono         , "PHONO"       },
            { InputSource.Cd            , "CD"          },
            { InputSource.Tuner         , "TUNER"       },
            { InputSource.Dvd           , "DVD"         },
            { InputSource.BlueRay       , "BD"          },
            { InputSource.Tv            , "TV"          },
            { InputSource.SatCbl        , "SAT/CBL"     },
            { InputSource.MediaPlayer   , "MPLAY"       },
            { InputSource.Game          , "GAME"        },
            { InputSource.HdRadio       , "HDRADIO"     },
            { InputSource.Net           , "NET"         },
            { InputSource.Pandora       , "PANDORA"     },
            { InputSource.SiriusXm      , "SIRIUSXM"    },
            { InputSource.Spotify       , "SPOTIFY"     },
            { InputSource.LastFm        , "LASTFM"      },
            { InputSource.Flickr        , "FLICKR"      },
            { InputSource.IRadio        , "IRADIO"      },
            { InputSource.Server        , "SERVER"      },
            { InputSource.Favorites     , "FAVORITES"   },
            { InputSource.Aux1          , "AUX1"        },
            { InputSource.Aux2          , "AUX2"        },
            { InputSource.Aux3          , "AUX3"        },
            { InputSource.Aux4          , "AUX4"        },
            { InputSource.Aux5          , "AUX5"        },
            { InputSource.Aux6          , "AUX6"        },
            { InputSource.Aux7          , "AUX7"        },
            { InputSource.BlueTooth     , "BT"          },
            { InputSource.UsbIpod       , "USB/IPOD"    },
            { InputSource.Usb           , "USB"         },
            { InputSource.Ipd           , "IPD"         },
            { InputSource.Irp           , "IRP"         },
            { InputSource.Fvp           , "FVP"         },
        };

        #endregion

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _tcpClient.Close();
                    _networkStream.Close();
                }
                _disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}

