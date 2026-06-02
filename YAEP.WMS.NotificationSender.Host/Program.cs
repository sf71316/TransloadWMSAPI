using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using YAEP.WMS.NotificationSender.Host.Lib;

namespace YAEP.WMS.NotificationSender.Host
{
    class Program
    {
        static Timer _SchedulerTimer;
        static bool _IsRun;
        static void Main(string[] args)
        {
            _IsRun = false;
            _SchedulerTimer = new Timer();
            _SchedulerTimer.Enabled = true;
            _SchedulerTimer.Interval = 0.1 * 60 * 1000;
            _SchedulerTimer.Elapsed += _schedulerTimer_Elapsed;
            string line = "";
            do
            {
                Console.WriteLine("WMS Sender  has Stared.");
                line = Console.ReadLine();
            }
            while (line != "exit");
        }

        private static void _schedulerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
         
            if (!_IsRun)
            {
              
                _IsRun = true;
                SenderAgent agent = new SenderAgent();
                agent.Notify += Agent_Notify;
                agent.ExecuteAsync();
                _IsRun = false;
            }
           
        }

        private static void Agent_Notify(object sender, NotificationArg e)
        {
            Console.ForegroundColor = e.Color;
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}] {e.Message}");
        }
    }
}
