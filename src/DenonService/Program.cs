using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Topshelf;

namespace DenonService
{
    

    public class Program
    {
        public static void Main()
        {
            var rc = HostFactory.Run(x =>                                  
            {
                x.Service<DenonService>(s =>                               
                {
                    s.ConstructUsing(name => new DenonService());          
                    s.WhenStarted(tc => tc.Start());                       
                    s.WhenStopped(tc => tc.Stop());
                    s.WhenShutdown(tc => tc.Shutdown());
                });

                x.EnableShutdown();
                x.RunAsLocalSystem();
                x.StartAutomatically();
                //x.UseNLog();

                x.SetDescription("Denon service - controlling your denon receiver");                  
                x.SetDisplayName("Denon service");                                 
                x.SetServiceName("DenonService");

                x.EnableServiceRecovery(tc => tc.RestartService(1));
                
            });                                                            

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());  
            Environment.ExitCode = exitCode;
        }
    }
}
