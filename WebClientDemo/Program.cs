using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebClientDemo.Models;
using System.Net.Http.Formatting;
using System.Collections.Specialized;
using System.Web;
using WebClientDemo.Http;
using System.Web.Http;
using System.Windows.Threading;
using System.Timers;
using System.Globalization;
using WebClientDemo.ReminderLogic;

namespace WebClientDemo
{
    public delegate void ReminderEventHandler();


    class ReminderQueue
    {
        public static Queue<Reminder> notificationQueue = new Queue<Reminder>();
        public event ReminderEventHandler reminderEvent;

        private Reminder _reminder { get; set; }
        public static int notificationCount = 0;

        public static int eventTrigered = 0;
        public Reminder reminder
        {
            set
            {

                eventTrigered++;
                notificationQueue.Enqueue(value);
                Console.WriteLine($"The 'ProcessReminder' event trigered. Count {eventTrigered}");
                reminderEvent();

            }

            get
            {
                return reminder;
            }
        }




    }


    class ProcessReminder
    {

        private string testSlackChanellUrl = "https://hooks.slack.com/services/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";


        private volatile Object _dequeueLock = new object();
        AutoResetEvent ars = new AutoResetEvent(false);
        public void dequeueReminder()
        {

            bool resource = false;
            try
            {

                if (Monitor.TryEnter(_dequeueLock))
                {

                    resource=true;

                    while (ReminderQueue.notificationQueue.Any())
                    {

                        var EtsClient = new EtsHttpClient(new User(ReminderQueue.notificationQueue.Dequeue().userName, "J~q1Y74Y~="));
                        Task collectTheEtsUserData = Task.Run(() => EtsClient.collectEtsUserData());
                        collectTheEtsUserData.Wait();
                        Task sendNotification = Task.Run(() => EtsClient.SendNotification(testSlackChanellUrl));
                        sendNotification.Wait();
                        Program.dequeuedRemindres++;
                        Console.WriteLine($"ReminderQueue event handler success. ***************Dequeued count: {Program.dequeuedRemindres}");



                    }
                }
            }
            finally
            {
                if (resource)
                {
                    Monitor.Exit(_dequeueLock);
                }


            }



        }


    }


    class Program
    {

        public static string smapleData = @"C:\Users\OGI-LapTop\Desktop\SlackBot\akvelon-slack-ets-integration\WebClientDemo\Test\sampleData.txt";
        public static string smapleDataDir = @"C:\Users\OGI-LapTop\Desktop\SlackBot\akvelon-slack-ets-integration\WebClientDemo\Test\";



        static List<Reminder> dataSet { get; set; }
        public static DateTime Time { get; set; }
              



        public static volatile int notificationSentCount = 0;
        public static volatile int dequeuedRemindres = 0;
        public static volatile int reminderReset = 0;
        public static volatile bool remindersReinicialized = false;


        static void Main(string[] args)
        {
            //Initialize the dataSet watcher
            dataWatcher();

            AutoResetEvent autoEvent = new AutoResetEvent(false);
            TimeSpan delayTime = new TimeSpan(0, 0, 0);
            TimeSpan intervalTime = new TimeSpan(0, 0, 0, 1);



            //Load the sample data data
            dataSet=ReminderDataHandler.loadData(smapleDataDir);






            System.Threading.Timer stateTimer = new System.Threading.Timer(
            async o => await OnTimedEvent(o, dataSet), autoEvent, delayTime, intervalTime);

            autoEvent.WaitOne();



        }

        private static volatile Object timeEventLock = new object();
        private static volatile Object remindersResetLock = new object();
        private static volatile Object _resetRemindersLock = new object();
        private static async Task OnTimedEvent(object state, List<Reminder> data)
        {


            bool unlocked = false;
            try
            {
                if (Monitor.TryEnter(timeEventLock))
                {
                    unlocked=true;
                    Time=DateTime.Now;
                    





                    //Reinitialize the remindes
                    if (DateTime.Parse(Time.ToString("HH:mm:ss"))<=DateTime.Parse("11:59:00")&&!remindersReinicialized)
                    {
                        lock (remindersResetLock)
                        {
                            remindersReinicialized=true;
                            Console.WriteLine($"Com{Thread.CurrentThread.ManagedThreadId} Trigered");

                            ReminderDataHandler.resetRemindersState(dataSet, smapleData);


                        }



                    }
                    else
                    {

                        if (Time>=DateTime.Parse("11:59:00").AddHours(12)&&remindersReinicialized)
                        {
                            remindersReinicialized=false;
                        }



                        var reminderQueue = new ReminderQueue();
                        var processReminder = new ProcessReminder();



                        //Triggered every day. Multiple times during the day can be selected.
                        var daily = data
                        .Where(r => r.reminderType=="daily"&&r.notified==false&&DateTime.Parse(r.time.ToString("HH:mm"))<=DateTime.Parse(Time.ToString("HH:mm")));
                        await Task.FromResult(daily);


                        //Triggered once pear week on selected day, multiple times during the specified day can be selected.
                        var weekly = data
                       .Where(r => r.reminderType=="weekly"&&r.day==DateTime.Now.ToString("dddd")&&r.notified==false&&DateTime.Parse(r.time.ToString("HH:mm"))<=DateTime.Parse(Time.ToString("HH:mm")));
                        await Task.FromResult(weekly);


                        //Triggered once a month, on a selected date and time. Multiple times during the specified date can be selected.
                        var monthly = data
                       .Where(r => r.reminderType=="monthly"&&r.date.Value.ToString("dd.MM.yyyy")==Time.ToString("dd.MM.yyyy")&&r.notified==false&&DateTime.Parse(r.time.ToString("HH:mm"))<=DateTime.Parse(Time.ToString("HH:mm")));
                        await Task.FromResult(monthly);






                        Console.WriteLine($"Thread ID:{Thread.CurrentThread.ManagedThreadId}*****Reminders count: M:{monthly.Count()} W:{weekly.Count()}  D:{daily.Count()}*********Scaning remiders...Current time: {Time.ToString("HH:mm")}");






                        lock (daily)
                        {


                            if (daily.Any())
                            {
                                var updateData = new List<Reminder>();

                                Task<List<Reminder>> sendDailyNotifications = Task.Run(async () =>
                                {

                                    reminderQueue.reminderEvent+=processReminder.dequeueReminder;

                                    return await Task.Run(() =>
                                    {
                                        foreach (Reminder reminder in daily)
                                        {
                                            reminder.notified=true;

                                            Console.WriteLine("Sending a daily notifications");

                                            reminderQueue.reminder=reminder;
                                            updateData.Add(reminder);

                                        }
                                        return updateData;
                                    });

                                });

                                sendDailyNotifications.ContinueWith(antecedent =>
                                {

                                    if (sendDailyNotifications.Result.Any())
                                    {

                                        ReminderDataHandler.updateRemindesState(updateData, dataSet, smapleData);
                                        updateData.Clear();

                                    }


                                }, TaskContinuationOptions.OnlyOnRanToCompletion);



                            }


                        }


                        lock (monthly)
                        {


                            if (monthly.Any())
                            {
                                var updateData = new List<Reminder>();

                                Task<List<Reminder>> sendDailyNotifications = Task.Run(async () =>
                                {

                                    reminderQueue.reminderEvent+=processReminder.dequeueReminder;

                                    return await Task.Run(() =>
                                    {
                                        foreach (Reminder reminder in monthly)
                                        {
                                            reminder.notified=true;

                                            Console.WriteLine("Sending a daily notifications");

                                            reminderQueue.reminder=reminder;
                                            updateData.Add(reminder);

                                        }
                                        return updateData;
                                    });

                                });

                                sendDailyNotifications.ContinueWith(antecedent =>
                                {

                                    if (sendDailyNotifications.Result.Any())
                                    {

                                        ReminderDataHandler.updateRemindesState(updateData, dataSet, smapleData);
                                        updateData.Clear();

                                    }


                                }, TaskContinuationOptions.OnlyOnRanToCompletion);



                            }





                        }





                        lock (weekly)
                        {


                            if (weekly.Any())
                            {
                                var updateData = new List<Reminder>();

                                Task<List<Reminder>> sendDailyNotifications = Task.Run(async () =>
                                {

                                    reminderQueue.reminderEvent+=processReminder.dequeueReminder;

                                    return await Task.Run(() =>
                                    {
                                        foreach (Reminder reminder in weekly)
                                        {
                                            reminder.notified=true;

                                            Console.WriteLine("Sending a daily notifications");

                                            reminderQueue.reminder=reminder;
                                            updateData.Add(reminder);

                                        }
                                        return updateData;
                                    });

                                });

                                sendDailyNotifications.ContinueWith(antecedent =>
                                {

                                    if (sendDailyNotifications.Result.Any())
                                    {

                                        ReminderDataHandler.updateRemindesState(updateData, dataSet, smapleData);
                                        updateData.Clear();

                                    }


                                }, TaskContinuationOptions.OnlyOnRanToCompletion);



                            }




                        }
                    }



                }



            }
            finally
            {
                if (unlocked)
                {

                    Monitor.Exit(timeEventLock);
                }
            }

        }



        public static void dataWatcher()
        {
            //This simulates the database watcher
            DataSetWatcher(smapleDataDir);
        }




        public static void DataSetWatcher(string path)
        {
            // Create a new FileSystemWatcher and set its properties.


            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path=path;
            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            watcher.NotifyFilter=NotifyFilters.LastAccess|NotifyFilters.LastWrite
               |NotifyFilters.FileName|NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter="*.txt";
            RenamedEventHandler renamedCallback = (sender, arg) =>
            {
                Console.WriteLine("Data changed--------");
                watcher.Dispose();
                dataSet=ReminderDataHandler.loadData(smapleDataDir);

                dataWatcher();




            };


            FileSystemEventHandler changedCallback = (sender, arg) =>
            {
                //pauseOnchnage.WaitOne();
                watcher.Dispose();
                Console.WriteLine("Data changed--------");
                dataWatcher();

            };
            watcher.Changed+=changedCallback;

            watcher.Renamed+=renamedCallback;


            watcher.EnableRaisingEvents=true;


        }



    }


}
