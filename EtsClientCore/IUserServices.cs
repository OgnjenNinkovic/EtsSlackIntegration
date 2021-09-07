using EtsClientData;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static EtsClientData.EtsDataContext;

namespace EtsClientCore
{
   public interface IUserServices
    {
        string SlackBotToken();

        Task<User> UserDetails(string slackChannelId);
        Task<bool> CreateOrUpdateUser(User user,int userId=0,string slackChannelId ="");

        Task<bool> DeleteUser(string EtsUserName);






        Task<List<Reminder>> Reminders(string etsUserName);
        Task<bool> CreateOrUpdateReminder( Reminder reminder,string etsUserName,int reminderId =0);

        Task DeleteReminder(int reminderId);



        Task CreateOrUpdateReminderTimer(ReminderTime reminderTime,int reminderId, int reminderTimerId = 0);

        Task DeleteReminderTimer(int reminderTimeId);
        
     

        Task<List<ReminderTime>> RemindersTimers(int reminderId);





        //The EtsClient endpoints
        Task< List<User>> Users();

       Task< List<Reminder>> Reminders();


        Task<List<ReminderTime>> ReminderTimes();


   

        void UpdateReminderTimersState(ReminderTime update);


    }
}
