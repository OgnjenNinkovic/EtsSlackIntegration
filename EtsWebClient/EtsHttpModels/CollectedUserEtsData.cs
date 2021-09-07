using System;
using System.Collections.Generic;
using System.Text;

namespace EtsWebClient.EtsHttpModels
{


    public class CollectedUserEtsData
    {

        public EmployeeDetails EmployeeDetails { get; set; }
        public List<Missingtime> MissingTime { get; set; }
        public List<Project_Tasks> ProjectsTasks { get; set; }

        public CollectedUserEtsData()
        {

        }
    


        public CollectedUserEtsData( List<Missingtime> missingtimes,List<Project_Tasks> projectTasks)
        {
            this.MissingTime = missingtimes;
            this.ProjectsTasks = projectTasks;
            this.EmployeeDetails = new EmployeeDetails();
        }
    }

    public class Missingtime
    {
        public string Date { get; set; }
        public string SumOfTime { get; set; }
        public EtsTimeUnitData TimeUnits { get; set; }
    }

   
    public class Project_Tasks
    {
        public string ProjectTitle { get; set; }
        public string ProjectID { get; set; }
        public List<ProjectTasks> TaskTypes { get; set; }
    }

    public class Tasktype
    {
        public string project_id { get; set; }
        public List<Task_Types> task_types { get; set; }
    }

    public class Task_Types
    {
        public string id { get; set; }
        public string title { get; set; }
        public bool deleted { get; set; }
    }


}
