using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using DenonLib;

namespace DenonService
{
    public class DenonService
    {
        private readonly IDenonDevice _denonDevice;
        readonly Timer _timer;
        public DenonService()
        {
            _denonDevice = new TcpDenonDevice(Properties.Settings.Default.ip, Properties.Settings.Default.port);
        }

        public void Start()
        {
            _denonDevice.PowerOn();
        }

        public void Stop()
        {
        }

        public void Shutdown()
        {
            _denonDevice.PowerStandby();
        }
    }
}
