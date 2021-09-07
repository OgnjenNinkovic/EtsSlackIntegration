using EtsClientCore;
using EtsClientData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.EventArgs;
using static EtsClientData.EtsDataContext;
using Microsoft.Data.SqlClient;
using TableDependency.SqlClient.Base.Enums;
using EtsWebClient.MainTimer;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace WebClientDemo.ReminderLogic
{
    class ReminderDataHandler 
    {
        //data context
        private static EtsDataContext _context = new EtsDataContext();
        private static EtsClientServices _clientServices = new EtsClientServices(_context);
        private static IUserServices userServices = _clientServices;
        private static volatile List<ReminderData> _dataSet;
        public static List<ReminderData> DataSet {

            get
            {
                _dataSet = loadDataSet().Result;
                return _dataSet;
            }
          
        
        }
       


        //Load data
        static volatile Object _loadDataLock = new object();
        public static async Task<List<ReminderData>> loadDataSet()
        {
            List<EtsDataContext.User> users = await _context.Users.AsNoTracking().Select(u => u).ToListAsync();
            List<EtsDataContext.Reminder> reminders =await _context.Reminders.AsNoTracking().Select(r => r).ToListAsync();
            List<EtsDataContext.ReminderTime> remindersTimers =await _context.ReminderTimes.AsNoTracking().Select(t=>t).ToListAsync();
           
            lock (_loadDataLock)
            {
                var result = new List<ReminderData>();
                Task<List<ReminderData>> _loadData = Task.Run(() =>
                {




                    result = (from u in users
                              join r in reminders on u.UserId equals r.UserId
                              join t in remindersTimers on r.ReminderId equals t.ReminderId
                              select new ReminderData
                              { UserId = u.UserId,
                                  EtsUserName = u.EtsUserName,
                                  EtsPassword = u.EtsPassword,
                                  SlackChannelID = u.SlackChannelID,
                                  UserType = u.UserType,
                                  ReminderId = r.ReminderId,
                                  ReminderType = r.Type,
                                  ReminderUserId = r.UserId,
                                  Date = r.Date,
                                  Time = t.Time,
                                  ReminderTimerId = t.ReminderTimeId,
                                  Notified = bool.Parse(t.Notified)
                              }).ToList();


                    return result.ToList();



                });

             
               
             return _loadData.Result;

            }

           

        }
        private static List<ReminderData> updateDataSet(object entityUpdate, List<ReminderData> inMemoryData)
        {


            var result = new List<ReminderData>();

            string entityName = entityUpdate.GetType().Name;
            switch (entityName)
            {
                case "User":
                    Task<List<ReminderData>> _updateUserDataSet = Task.Run(async () =>
                    {
                        return await Task.Run(() =>
                            {
                                result = inMemoryData;
                                User update = (entityUpdate as User);
                                foreach (var item in result.Where(r => r.UserId == update.UserId))
                                {
                                    item.SlackChannelID = update.SlackChannelID;
                                    item.EtsUserName = update.EtsUserName;
                                    item.EtsPassword = update.EtsPassword;
                                    item.UserType = update.UserType;
                                }

                                return result;

                            });
                    });
                    result = _updateUserDataSet.Result;
                    break;
                case "Reminder":
                    Task<List<ReminderData>> _updateReminderDataSet = Task.Run(async () =>
                    {
                        return await Task.Run(() =>
                        {
                            result = inMemoryData;

                            Reminder update = (entityUpdate as Reminder);

                            foreach (var item in result.Where(r => r.ReminderId == update.ReminderId))
                            {
                                item.ReminderType = update.Type;
                                item.ReminderUserId = update.UserId;
                                item.Date = update.Date;
                               
                            }

                            return result;

                        });
                    });
                    result = _updateReminderDataSet.Result;

                    break;
                case "ReminderTime":
                    Task<List<ReminderData>> _updateReminderTimeDataSet = Task.Run(async () =>
                    {
                        return await Task.Run(() =>
                        {
                            result = inMemoryData;

                            ReminderTime update = (entityUpdate as ReminderTime);

                            foreach (var item in result.Where(r => r.ReminderTimerId == update.ReminderTimeId))
                            {
                              
                                item.Notified = bool.Parse(update.Notified);
                                item.Time = update.Time;
                            }

                            return result;

                        });
                    });
                    result = _updateReminderTimeDataSet.Result;
                    break;
                default:
                    break;
            }
             return result;

           


        }



        //Reset rminders state
        static volatile Object _resetReminderStateLock = new object();
        public static void resetRemindersState(List<ReminderData> dataSet)
        {
            Debug.WriteLine("*****************************************************************Reinitializing reminders*********************************");

            //Implement logic for reinitializing weekly and monthly reminders

            //reset monthly every month
            //reset weekly every week

            //reset daily every day
            lock (_resetReminderStateLock)
            {

                Task _resetRemindersState = Task.Run(async () =>
                {
                    List<ReminderData> data = dataSet.OrderBy(i => i.ReminderTimerId).ToList();


                    await Task.Run(() =>
                    {
                        //Reinitialize daily reminders
                        if (data.Where(r => r.ReminderType == "daily").Any())
                        {
                            foreach (var reminder in data.Where(r => r.ReminderType == "daily"))
                            {
                                reminder.Notified = false;
                                var update = new EtsClientData.EtsDataContext.ReminderTime { ReminderTimeId = reminder.ReminderTimerId, Time = reminder.Time, Notified = reminder.Notified.ToString() };

                                userServices.UpdateReminderTimersState(update);
                            }
                        }



                        //Reinitialize weekly reminders
                        if (data.Where(r => r.ReminderType == "weekly").Any() && StatusChecker.Time.ToString("dddd") == "Monday")
                        {
                            foreach (var reminder in data.Where(r => r.ReminderType == "weekly"))
                            {
                                reminder.Notified = false;
                                var update = new EtsClientData.EtsDataContext.ReminderTime { ReminderTimeId = reminder.ReminderTimerId, Time = reminder.Time, Notified = reminder.Notified.ToString() };

                                userServices.UpdateReminderTimersState(update);
                            }
                        }

                        //Reinitialize monthly reminders
                        if (data.Where(r => r.ReminderType == "monthly").Any() && StatusChecker.Time.Day == 1)
                        {
                            foreach (var reminder in data.Where(r => r.ReminderType == "monthly"))
                            {
                                reminder.Notified = false;
                                var update = new EtsClientData.EtsDataContext.ReminderTime { ReminderTimeId = reminder.ReminderTimerId, Time = reminder.Time, Notified = reminder.Notified.ToString() };

                                userServices.UpdateReminderTimersState(update);
                            }
                        }

                    });


                });
                Debug.WriteLine("*****************************************************************Reminders reinitialization complete*********************************");

                _resetRemindersState.Wait();

            }




        }

        //Update the specific reminder with provided data
        private volatile static Object _updateRemindersStateLock = new object();
        public static void updateRemindesState(List<ReminderData> remindersUpdates)
        {
            lock (_updateRemindersStateLock)
            {
                Task _updateRemindersState = Task.Run(() =>
                {


                    foreach (ReminderData reminder in remindersUpdates)
                    {

                        reminder.Notified = true;
                        var update = new EtsClientData.EtsDataContext.ReminderTime { ReminderTimeId = reminder.ReminderTimerId, Time = reminder.Time, Notified = reminder.Notified.ToString() };

                        userServices.UpdateReminderTimersState(update);
                    }




                });

                _updateRemindersState.Wait();
            }




        }




        private static volatile Object _dataSetWatcherLock = new object();

        public static void DatabaseWatcher()
        {
            lock (_dataSetWatcherLock)
            {
            
                //EtsDataContext etsDataContext = new EtsDataContext();
                //var reminderTimers_Table_dependency = new SqlTableDependency<EtsDataContext.ReminderTime>(etsDataContext.ConnectionString, "ReminderTimes");
                //var reminders_Table_dependency = new SqlTableDependency<EtsDataContext.Reminder>(etsDataContext.ConnectionString, "Reminders");
                //var users_Table_dependency = new SqlTableDependency<EtsDataContext.User>(etsDataContext.ConnectionString, "Users");


                //users_Table_dependency.OnChanged += Users_Table_dependency_OnChanged;
                //users_Table_dependency.OnError += Table_dependency_OnError;

                //reminders_Table_dependency.OnChanged += Reminders_Table_dependency_OnChanged;
                //reminders_Table_dependency.OnError += Table_dependency_OnError;


                //reminderTimers_Table_dependency.OnChanged +=ReminderTime_table_dependency_Changed;
                //reminderTimers_Table_dependency.OnError += Table_dependency_OnError;


                //users_Table_dependency.Start();
                //reminders_Table_dependency.Start();
                //reminderTimers_Table_dependency.Start();

                //StatusChecker.DataSet = EtsClientData.EtsDataContext.ReminderData;

                EtsDataContext.User.userEntityChanged += User_userEntityChanged;
                EtsDataContext.Reminder.reminderEntityChanged += Reminder_reminderEntityChanged;
                EtsDataContext.ReminderTime.reminderTimeEntityChanged += ReminderTime_reminderTimerChanged;

                Debug.WriteLine("DataBaseWatecher Started!");
            }



        }

        private static void ProcessEntityEvent<TEntity>(object e) where TEntity : class
        {
            string eventType = e.GetType().Name;


            if (eventType.Contains("DeletedEntry"))
            {
                var ev = (EntityFrameworkCore.Triggers.IDeletedEntry<TEntity>)e;
                StatusChecker.DataSet = ReminderDataHandler.loadDataSet().Result;
                Debug.WriteLine($"The record is removed from the entity. ID:{ev.Entity}");

            }
            else if (eventType.Contains("InsertedEntry"))
            {
                var ev = (EntityFrameworkCore.Triggers.IInsertedEntry<TEntity>)e;
                StatusChecker.DataSet = ReminderDataHandler.loadDataSet().Result;
                Debug.WriteLine($"The record is added to the entity.  ID:{ev.Entity}");

            }
            else if (eventType.Contains("UpdatedEntry"))
            {

                var ev = (EntityFrameworkCore.Triggers.IUpdatedEntry<TEntity>)e;
                StatusChecker.DataSet = updateDataSet(ev.Entity, StatusChecker.DataSet);
                Debug.WriteLine($"The entity has been updated.  ID:{ev.Entity}");

            }
        }

        private static void User_userEntityChanged(object e)
        {
            ProcessEntityEvent<User>(e);
        }

        private static void Reminder_reminderEntityChanged(object e)
        {
            ProcessEntityEvent<Reminder>(e);
        }


        private static void ReminderTime_reminderTimerChanged(object e)
        {
            ProcessEntityEvent<ReminderTime>(e);
        }


     



        private static void Users_Table_dependency_OnChanged(object sender, RecordChangedEventArgs<EtsDataContext.User> e)
        {
          
            try
            {
                lock (_dataSetWatcherLock)
                {
                    switch (e.ChangeType)
                    {
                        case ChangeType.Insert:
                            {
                                StatusChecker.DataSet = ReminderDataHandler.loadDataSet().Result;
                                Debug.WriteLine("Insert");
                          
                            }
                            break;

                        case ChangeType.Update:
                            {

                                StatusChecker.DataSet = updateDataSet(e.Entity,StatusChecker.DataSet);
                                Debug.WriteLine($"Update the value of notified: {e.Entity.EtsUserName}");
                            }
                            break;

                        case ChangeType.Delete:
                            {
                                StatusChecker.DataSet = ReminderDataHandler.loadDataSet().Result;
                                Debug.WriteLine("Delete");
                            }
                            break;
                    };
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private static void Reminders_Table_dependency_OnChanged(object sender, RecordChangedEventArgs<EtsDataContext.Reminder> e)
        {
            try
            {

                switch (e.ChangeType)
                {
                    case ChangeType.Insert:
                        {
                            StatusChecker.DataSet = ReminderDataHandler.loadDataSet().Result;
                            Debug.WriteLine("Insert");

                        }
                        break;

                    case ChangeType.Update:
                        {
                            StatusChecker.DataSet = updateDataSet(e.Entity, StatusChecker.DataSet);
                            Debug.WriteLine($"Update the value of notified: {e.Entity.Type}");
                        }
                        break;

                    case ChangeType.Delete:
                        {
                            StatusChecker.DataSet = ReminderDataHandler.loadDataSet().Result;
                            Debug.WriteLine("Delete");
                        }
                        break;
                };



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



        private static void ReminderTime_table_dependency_Changed(object sender, RecordChangedEventArgs<EtsDataContext.ReminderTime> e)
        {
            try
            {
                lock (_dataSetWatcherLock)
                {
                    switch (e.ChangeType)
                    {
                        case ChangeType.Insert:
                            {
                                StatusChecker.DataSet = ReminderDataHandler.loadDataSet().Result;
                                Debug.WriteLine("Insert");

                            }
                            break;

                        case ChangeType.Update:
                            {
                                StatusChecker.DataSet = updateDataSet(e.Entity, StatusChecker.DataSet);
                                Debug.WriteLine($"Update the value of notified: {e.Entity.Notified}");
                            }
                            break;

                        case ChangeType.Delete:
                            {
                                StatusChecker.DataSet = ReminderDataHandler.loadDataSet().Result;
                                Debug.WriteLine("Delete");
                            }
                            break;
                    };
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }




        private static void Table_dependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
        {
            Exception ex = e.Error;
           
            Debug.WriteLine(ex);
        }











    }
}
