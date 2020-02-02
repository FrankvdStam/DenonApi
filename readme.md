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


TcpForward can be used to forward all commands over TCP. It has no knowledge about the commands and does just forwarding.

Example usage:

	C:\projects\DenonApi\src\TcpForward\bin\Debug\netcoreapp3.0>tcpforward.exe 192.168.1.65 23 PWON
	Parsed input as ip: 192.168.1.65, port: 23 and command: PWON

	C:\projects\DenonApi\src\TcpForward\bin\Debug\netcoreapp3.0>tcpforward.exe 192.168.1.65 23 MVUP
	Parsed input as ip: 192.168.1.65, port: 23 and command: MVUP