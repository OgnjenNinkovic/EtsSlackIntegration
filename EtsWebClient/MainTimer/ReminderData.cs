using System;
using System.Collections.Generic;
using System.Text;

namespace EtsClientData
{


    public class ReminderData
    {
        public int UserId { get; set; }
        public string EtsUserName;
        public string EtsPassword { get; set; }

        public string SlackChannelID { get; set; }

        public string UserType { get; set; }

        public int ReminderId { get; set; }
        public string ReminderType { get; set; }

        public int ReminderUserId { get; set; }
        public DateTime Date { get; set; }
        public DateTime Time { get; set; }
        public int ReminderTimerId { get; set; }
        public bool Notified { get; set; }
    }


}
