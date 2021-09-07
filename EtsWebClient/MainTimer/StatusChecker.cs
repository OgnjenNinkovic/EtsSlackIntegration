using EtsClientData;
using EtsWebClient.EtsHttpModels;
using EtsWebClient.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using WebClientDemo.ReminderLogic;

namespace EtsWebClient.MainTimer
{
    class StatusChecker
    {


       internal static TimeZoneInfo TimeZone { get; set; }

        public static string Test { get; set; }
        public static List<ReminderData> DataSet { get; set; }
        private static volatile Object _processRemindersLock = new object();
        private static volatile bool remindersReinicialized = false;
        public static DateTime Time { get; set; }

       
        public StatusChecker(List<ReminderData> dataSet, string timeZone = "Central Europe Standard Time")
        {

            if (dataSet != null)
            {
                DataSet = dataSet;

            }
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), TimeZone);

        }

        // This method is called by the timer delegate.
        public void RemindUsers(Object stateInfo)
        {


            AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;

            lock (_processRemindersLock)
            {
                //Check reinitialized state
                CheckRemindersInitializationState();


                //Calculate next reminder time
                if (NextReminderTime(DataSet) != null)
                {

                    Debug.WriteLine($"Sending reminders trigered");

                    List<ReminderData> remindersForSending = new List<ReminderData>();
                    remindersForSending.Add(NextReminderTime(DataSet));
                    SendReminder(remindersForSending);
                    remindersForSending.Clear();

                }

            }


        }
        private void CheckRemindersInitializationState()
        {
            lock (_processRemindersLock)
            {
                Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), TimeZone);

                if (Time <= new DateTime(Time.Year, Time.Month, Time.Day, 11, 59, 00) && !remindersReinicialized)
                {
                    remindersReinicialized = true;
                    ReminderDataHandler.resetRemindersState(DataSet);
                    Debug.WriteLine($"Com{Thread.CurrentThread.ManagedThreadId} Trigered");
                }
            }

        }

        private ReminderData NextReminderTime(List<ReminderData> data)
        {
            lock (_processRemindersLock)
            {
                Time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), TimeZone);
               
                ReminderData smallest;


                var t = Time.Hour;
                //Reinitialize reminders
                if (Time >= new DateTime(Time.Year, Time.Month, Time.Day, 11, 59, 00).AddHours(12) && remindersReinicialized)
                {
                    remindersReinicialized = false;
                }

                //Triggered every day. Multiple times during the day can be selected.
                var daily = data.Where(r => r.ReminderType == "daily" && r.Notified == false && r.Time.TimeOfDay <= Time.TimeOfDay)
                            .Aggregate(null as ReminderData, (smallest, next) =>
                            {
                                if (smallest == null || smallest.Time > next.Time)
                                {
                                    smallest = next;
                                }
                                return smallest;
                            });


                //Triggered once pear week on selected day, multiple times during the specified day can be selected.
                var weekly = data.Where(r => r.ReminderType == "weekly" && r.Date.ToString("dddd") == Time.ToString("dddd") && r.Notified == false && r.Time.TimeOfDay <= Time.TimeOfDay)
                                     .Aggregate(null as ReminderData, (smallest, next) =>
                                     {
                                         if (smallest == null || smallest.Time > next.Time)
                                         {
                                             smallest = next;
                                         }
                                         return smallest;
                                     });
                smallest = daily == null ? weekly : (weekly == null ? daily : (daily.Time < weekly.Time ? daily : weekly));

                //Triggered once a month, on a selected date and time. Multiple times during the specified date can be selected.
                var monthly = data.Where(r => r.ReminderType == "monthly" && r.Date.ToString("dd") == Time.ToString("dd") && r.Notified == false && r.Time.TimeOfDay <= Time.TimeOfDay)
                                  .Aggregate(null as ReminderData, (smallest, next) =>
                                  {
                                      if (smallest == null || smallest.Time > next.Time)
                                      {
                                          smallest = next;
                                      }
                                      return smallest;
                                  });

                if (smallest == null || (monthly != null && smallest.Time > monthly.Time))
                {
                    smallest = monthly;
                }
                return smallest;



            }


        }





        public static void SendReminder(IEnumerable<ReminderData> reminders)
        {
            var databaseUpdate = new List<ReminderData>();

            Debug.WriteLine("Sending reminder...");
            lock (_processRemindersLock)
            {
                Task<List<ReminderData>> sendDailyNotifications = Task.Run(() =>
                {

                    //Send notification
                    foreach (ReminderData reminder in reminders.ToList())
                    {

                        var EtsClient = new EtsHttpClient(new EtsUser(reminder.EtsUserName, reminder.EtsPassword, reminder.SlackChannelID));
                        EtsClient.collectEtsUserData().ContinueWith((collectData) =>
                        {

                            if (collectData.Result)
                            {
                                EtsClient.SendNotification(reminder.SlackChannelID);
                            }

                        }, TaskContinuationOptions.OnlyOnRanToCompletion);



                        databaseUpdate.Add(reminder);
                    }

                    return databaseUpdate;
                });



                //Update database

                Task updateDatabase = sendDailyNotifications.ContinueWith(antecedent =>
                {
                    ReminderDataHandler.updateRemindesState(antecedent.Result);
                    databaseUpdate.Clear();

                }, TaskContinuationOptions.OnlyOnRanToCompletion);

                updateDatabase.Wait();
            }



        }


    }
}
