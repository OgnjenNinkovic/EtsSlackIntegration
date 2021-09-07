using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebClientDemo.ReminderLogic
{

    //string eventRow = "ID,userName,reminderType,time ";
    public class Reminder
    {
        public int id;
        public string userName;
        public string reminderType;
        public string day;
        public DateTime? date;
        public DateTime time;
        public bool notified;


        public static bool TryParse(string text, out Reminder re)
        {
            re=new Reminder
            {
                id=int.MinValue,
                userName=string.Empty,
                reminderType=string.Empty,
                day=string.Empty,
                date=DateTime.MinValue,
                time=DateTime.MinValue,
                notified=false

            };
            var data = text.Split(' ');


            if (data.Length!=7)
                return false;


            //ID
            if (!int.TryParse(data[0], out int _id))
                return false;


            //UserName
            if (String.IsNullOrEmpty(data[1]))
                return false;


            //ReminderType
            if (String.IsNullOrEmpty(data[2]))
                return false;

            //Day
            if (String.IsNullOrEmpty(data[3]))
                return false;

            //Date
            if (!DateTime.TryParse(data[4], out DateTime _date))
                return false;


            //Time
            if (!DateTime.TryParse(data[5], out DateTime _time))
                return false;


            //Notified
            if (!bool.TryParse(data[6], out bool _notified))
                return false;


            re=new Reminder
            {
                id=_id,
                userName=data[1],
                reminderType=data[2],
                day=data[3],
                date=_date,
                time=_time,
                notified=_notified
            };

            return true;

        }

    }
}
