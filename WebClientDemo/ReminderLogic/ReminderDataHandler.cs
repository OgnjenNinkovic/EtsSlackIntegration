using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebClientDemo.ReminderLogic
{
    class ReminderDataHandler
    {

        //Load data
       static volatile Mutex mutex = new Mutex();
        static volatile Object _loadDataLock = new object();

        public static List<Reminder> loadData(string dataConnectionString)
        {
            Thread.Sleep(100);
            lock (_loadDataLock)
            {



                var result = new List<Reminder>();


                Task<List<Reminder>> _loadData = Task.Run(async () =>
                {

                    return await Task.Run(() =>
                    {

                        if (dataConnectionString!=null)
                        {
                            string file = Directory.GetFiles(dataConnectionString, "*.txt", SearchOption.TopDirectoryOnly).FirstOrDefault();

                            using (var text = new StreamReader(file))
                            {

                                // ignore 1st line of text, it contains headers.


                                text.ReadLine();

                                result=ReminderDataHandler.ReadAll(text).ToList();

                      



                            }


                        }
                        else
                        {
                            Console.WriteLine("No data loded, the flie location returns null");
                        }
                        return result;

                    });

                });

                result=_loadData.Result;

                return result;
            }

        }

        //Reset rminders state
        static volatile Object _resetReminderStateLock = new object();
        public static void resetRemindersState(List<Reminder> dataSet, string dataConnectionString)
        {
            Console.WriteLine("*****************************************************************Reinitializing reminders*********************************");

            //Implement logic for reinitializing weekly and monthly reminders

            //reset monthly every month
            //reset weekly every week

            //reset daily every day
            lock (_resetReminderStateLock)
            {

                Task _resetRemindersState = Task.Run(async () =>
                {
                    List<Reminder> data = dataSet.OrderBy(i => i.id).ToList();

                
                    await Task.Run(() =>
                    {
                        //Reinitialize daily reminders
                        if (data.Where(r => r.reminderType=="daily").Any())
                        {
                            foreach (var dailyReminder in data.Where(r => r.reminderType=="daily"))
                            {
                                dailyReminder.notified=false;
                            }
                        }

                        string day = Program.Time.ToString("dddd");
                        //Reinitialize weekly reminders
                        if (data.Where(r => r.reminderType=="weekly").Any()&& Program.Time.ToString("dddd")=="Monday")
                        {
                            foreach (var dailyReminder in data.Where(r => r.reminderType=="weekly"))
                            {
                                dailyReminder.notified=false;
                            }
                        }

                        //Reinitialize monthly reminders
                        if (data.Where(r => r.reminderType=="monthly").Any()&&Program.Time.Day==1)
                        {
                            foreach (var dailyReminder in data.Where(r => r.reminderType=="monthly"))
                            {
                                dailyReminder.notified=false;
                            }
                        }

                    });


                    using (StreamWriter writer = new StreamWriter(dataConnectionString))
                    {
                        writer.WriteLine($"id userName reminderType day date time notified");
                        await Task.Run(() =>
                        {
                            foreach (Reminder line in data)
                            {
                                writer.WriteLine($"{line.id} {line.userName} {line.reminderType} {line.day} {line.date.Value.ToString("dd.MM.yyyy")} {line.time.ToString("HH:mm")} {line.notified}");

                            }
                        });

                    }

                    Thread.Sleep(20000);
                });

                _resetRemindersState.Wait();
                Console.WriteLine("*****************************************************************Reminders reinitialization complete*********************************");

            }






        }

        //Update the specific reminder with provided data
        private volatile static Object _updateRemindersStateLock = new object();
        public static void updateRemindesState(List<Reminder> remindersUpdates, List<Reminder> dataSet, string dataConnectionString)
        {
            lock (_updateRemindersStateLock)
            {
                Task _updateRemindersState = Task.Run(async () =>
                {

                    await Task.Run(() =>
                    {
                        for (int i = 0; i<remindersUpdates.Count; i++)
                        {
                            if (dataSet.ElementAt(i)==remindersUpdates.Select(r => r.id))
                            {
                                dataSet.ElementAt(i).notified=remindersUpdates.ElementAt(i).notified;
                            }
                        }

                        List<Reminder> sortedData = dataSet.OrderBy(i => i.id).ToList();


                        using (StreamWriter writer = new StreamWriter(dataConnectionString))
                        {

                            writer.WriteLine($"id userName reminderType day date time notified");
                            foreach (Reminder line in sortedData)
                            {
                                writer.WriteLine($"{line.id} {line.userName} {line.reminderType} {line.day} {line.date.Value.ToString("dd.MM.yyyy")} {line.time.ToString("HH:mm")} {line.notified}");

                            }

                        }

                    });


                });

                _updateRemindersState.Wait();
            }
            


        }


        private static IEnumerable<Reminder> ReadAll(TextReader text, Action<string> errorHandler = null)
        {
            string line = null;
            while ((line=text.ReadLine())!=null)
            {
                if (Reminder.TryParse(line, out Reminder wo))
                {
                    yield return wo;
                }
                else
                {
                    try
                    {

                        errorHandler?.Invoke(line);

                    }
                    catch { };
                }
            }
        }
    }
}
