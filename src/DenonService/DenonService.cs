using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using DenonLib;
using Microsoft.Win32;

namespace DenonService
{
    public class DenonService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IDenonDevice _denonDevice;
        public DenonService()
        {
            _denonDevice = new TcpDenonDevice(Properties.Settings.Default.ip, Properties.Settings.Default.port);
        }

        public void Shutdown()
        {
            _denonDevice.PowerStandby();
        }
        
        public void Start()
        {
            _denonDevice.PowerOn();
        }

        public void Stop()
        {
        }
    }
}
