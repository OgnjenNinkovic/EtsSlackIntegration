using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtsWebClient.EtsHttpModels
{
    class Notification
    {
        public string employeeUserName { get; set; }
        public string scheduledTime { get; set; }

        public List<Project> projects { get; set; }

        public List<ProjectTasks> projectTasks { get; set; }

        public List<TimeEntry>  dateTime { get; set; }

        public List<EtsTimeUnitData> timeUnits { get; }

       
        public Notification(List<TimeEntry> dateTime, List<EtsTimeUnitData> timeUnits, List<Project> projects, List<ProjectTasks> projectTasks)
        {
           
            this.dateTime = dateTime;
            this.timeUnits = timeUnits;
            this.projects = projects;
            this.projectTasks = projectTasks;

            
           
        }

    }


    class TimeEntry
    {
        public string date { get; set; }

        public string sumOfTimeUnits { get; set; }
    }

}
