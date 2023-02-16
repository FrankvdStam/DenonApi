# DenonApi

![](https://github.com/FrankvdStam/DenonApi/workflows/Build/badge.svg)


Lightweight library to control Denon AVR devices via TCP.
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
	Parsed input as ip: 192.168.1.65, port: 23, command: PWON, wait for response: False, write to file:

	Parsed input as ip: 192.168.1.65, port: 23, command: MVUP, wait for response: True, write to file: C:\Temp\denon.txt
	MVMAX 91: MV55
	File doesn't exist - not writing.

	C:\projects\DenonApi\src\TcpForward\bin\Debug\netcoreapp3.0>tcpforward.exe 192.168.1.65 23 MVUP w C:\Temp\denon.txt
	Parsed input as ip: 192.168.1.65, port: 23, command: MVUP, wait for response: True, write to file: C:\Temp\denon.txt
	MVMAX 91: MV555

	C:\projects\DenonApi\src\TcpForward\bin\Debug\netcoreapp3.0>tcpforward.exe 192.168.1.65 23 MVUP w C:\Temp\denon.txt
	Parsed input as ip: 192.168.1.65, port: 23, command: MVUP, wait for response: True, write to file: C:\Temp\denon.txt
	MVMAX 91: MV56

	C:\projects\DenonApi\src\TcpForward\bin\Debug\netcoreapp3.0>tcpforward.exe 192.168.1.65 23 MVUP w C:\Temp\denon.txt
	Parsed input as ip: 192.168.1.65, port: 23, command: MVUP, wait for response: True, write to file: C:\Temp\denon.txt
	MVMAX 91: MV565
