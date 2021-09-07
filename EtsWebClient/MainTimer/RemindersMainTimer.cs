using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebClientDemo.ReminderLogic;

namespace EtsWebClient.MainTimer
{
   public class RemindersMainTimer
    {
        
        internal static volatile int notificationSentCount = 0;
        internal static volatile int dequeuedRemindres = 0;
        internal static volatile int reminderReset = 0;

        private static AutoResetEvent autoEvent = new AutoResetEvent(false);
        private static StatusChecker StatusChecker = new StatusChecker(ReminderDataHandler.DataSet);

        //Main timer parametars
        private static Timer stateTimer;
        private static TimeSpan _delayTime;
        private static TimeSpan _intervalTime;

        public static TimeZoneInfo ManTimerTimeZone { get; private set; } = StatusChecker.TimeZone;
         

        public RemindersMainTimer(TimeSpan delayTime = default, TimeSpan intervalTime = default)
        {
            _delayTime = delayTime;
            _intervalTime = intervalTime;
        }

        public void StartMainTimerAsync()
        {
           
            Task.Factory.StartNew(()=> {
            
            //Initialize the database watcher
             ReminderDataHandler.DatabaseWatcher();


            // Create the delegate that invokes methods for the timer.
            TimerCallback timerDelegate = new TimerCallback(StatusChecker.RemindUsers);


            if (_delayTime == TimeSpan.Zero || _intervalTime == TimeSpan.Zero)
            {
                _delayTime = new TimeSpan(0, 0, 0);
                _intervalTime = new TimeSpan(0, 0, 0, 1);
                stateTimer = new Timer(timerDelegate, autoEvent, _delayTime, _intervalTime);

            }
            else
            {
                stateTimer = new Timer(timerDelegate, autoEvent, _delayTime, _intervalTime);

            }

            autoEvent.WaitOne();
            
            });
           

        }

     

        public void DoQuit()
        {
            stateTimer.Dispose();
            autoEvent.Set();
           
        }

    }
}
