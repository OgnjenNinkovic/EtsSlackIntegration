using EtsClientData;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static EtsClientData.EtsDataContext;


namespace EtsClientCore
{
    public class EtsClientServices : IUserServices
    {
        private EtsDataContext _context;
        private string _SlackBotToken { get; set; } = "xoxb-1977180709686-2224795510705-zCcF5BkrNV9cig5TOAuBIoVp";

        
        public EtsClientServices(EtsDataContext context)
        {
            _context = context;

        }

        public string SlackBotToken()
        {
            return _SlackBotToken;
        }

        //User CRUD

        public async Task<List<User>> Users()
        {
            
            return await _context.Users.ToListAsync();
          

        }
        public async Task<bool> CreateOrUpdateUser(User user,int userId=0, string slackChannelId = "")
        {
            if (userId == 0 && string.IsNullOrEmpty(slackChannelId))
            {
                 await _context.AddAsync(user);
                 await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                string userIdentifyer = userId != 0 ? userId.ToString() : slackChannelId;
                var userForUpdate = await _context.Users.FirstOrDefaultAsync(u => u.UserId.ToString().Equals(userIdentifyer)) ??
                   await _context.Users.FirstOrDefaultAsync(u => u.SlackChannelID.Equals(userIdentifyer));
                if (userForUpdate != null)
                {
                    userForUpdate.EtsPassword = user.EtsPassword;
                    userForUpdate.EtsUserName = user.EtsUserName;
                    userForUpdate.SlackChannelID = user.SlackChannelID;

                     _context.Update(userForUpdate);
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        public async  Task<User> UserDetails(string slackChannelId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.SlackChannelID == slackChannelId);

            return user;
        }


        public async Task<bool> DeleteUser(string EtsUserName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u=>u.EtsUserName == EtsUserName);

            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
           
           
        }




   
        //Reminder CRUD
        public async  Task<List<Reminder>> Reminders(string EtsUserName)
        {
            var userId = await _context.Users.FirstOrDefaultAsync(u => u.EtsUserName == EtsUserName);

            return await _context.Reminders.Where(r => r.UserId == userId.UserId).ToListAsync();
                          


        }



        public async Task<bool> CreateOrUpdateReminder(Reminder reminder,string etsUserName,int reminderID =0 )
        {
            if (reminderID ==0)
            {
                var databaseUserId = await _context.Users.FirstOrDefaultAsync(u => u.EtsUserName == etsUserName);
                reminder.UserId = databaseUserId.UserId;
                await _context.AddAsync(reminder);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
              
                var reminderForUpdate = await _context.Reminders.FindAsync(reminderID);
                if (!reminderForUpdate.Equals(null) )
                {
                    reminderForUpdate.Type = reminder.Type;
                    reminderForUpdate.Date = reminder.Date;
                    _context.Update(reminderForUpdate);
                    await _context.SaveChangesAsync();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }



        public async Task DeleteReminder(int reminderId)
        {
            var remiderForDelition = await _context.Reminders.FindAsync(reminderId);
            if (remiderForDelition != null)
            {
                _context.Reminders.Remove(remiderForDelition);
                await _context.SaveChangesAsync();

            }
        }










        //ReminderTimers CRUD
        public async Task CreateOrUpdateReminderTimer(ReminderTime reminderTime,int reminderId, int reminderTimerId = 0)
        {
            if (reminderTimerId == 0)
            {
                reminderTime.ReminderId = reminderId;
                _context.ReminderTimes.Add(reminderTime);
                await _context.SaveChangesAsync();

            }
            else
            {
                var timerForUpdate = await _context.ReminderTimes.FindAsync(reminderTimerId);
                timerForUpdate.Time = reminderTime.Time;
                timerForUpdate.Notified = reminderTime.Notified;
                 _context.ReminderTimes.Update(timerForUpdate);
                await _context.SaveChangesAsync();
            }
        }



        public async Task DeleteReminderTimer(int reminderTimeId)
        {
           var timer = await _context.ReminderTimes.FindAsync(reminderTimeId);
            if (timer != null)
            {
                 _context.ReminderTimes.Remove(timer);
               await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ReminderTime>> RemindersTimers(int reminderID)
        {
            Task<List<ReminderTime>> timers = Task.Run(() => {
                var reminderTimers =  from t in _context.ReminderTimes
                                       where t.ReminderId == reminderID
                                       select t;
                                    
                return reminderTimers.ToList();
            });


            return await timers;


        }


        public async Task<List<Reminder>> Reminders()
        {
           
            return await _context.Reminders.ToListAsync();
        
        }



        public async Task< List<ReminderTime>> ReminderTimes()
        {
            return await _context.ReminderTimes.ToListAsync();
        }



    

      
        public async void UpdateReminderTimersState(ReminderTime update)
        {
          
                var reminderTimerUpdate = Task.Run(() =>
                {
                    return _context.FindAsync<ReminderTime>(update.ReminderTimeId).Result;

                });
                await Task.FromResult(reminderTimerUpdate.Result);
              reminderTimerUpdate.Wait();

                Task updateDataBase = reminderTimerUpdate.ContinueWith((t) =>
                {
                    t.Result.Notified = update.Notified;
                    t.Result.Time = update.Time;
                    _context.Update(t.Result);
                    _context.SaveChanges();

                },TaskContinuationOptions.OnlyOnRanToCompletion);
            updateDataBase.Wait();

        }

       
    }
}
