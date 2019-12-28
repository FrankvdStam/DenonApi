# DenonApi

![](https://github.com/FrankvdStam/DenonApi/workflows/Build/badge.svg)


Lightweight library for controlling Denon AVR devices via TCP.
Consider everything unstable in this library.

Example usage:

    class Program
    {
        static void Main(string[] args)
        {
            IDenonDevice denonDevice = new TcpDenonDevice("192.168.1.65", 23);
            denonDevice.PowerOn();
            Thread.Sleep(10000); //let the device boot
            denonDevice.MasterVolumeUp();
            denonDevice.MasterVolumeDown();
            denonDevice.SetMasterVolume(51);
            denonDevice.ChannelVolumeUp(Channel.SubWoofer1);
            denonDevice.SetChannelVolume(Channel.SubWoofer1, 51.5M);
            Dictionary<Channel, decimal> result = denonDevice.GetChannelStatus();
            denonDevice.PowerStandby();
        }
    }
