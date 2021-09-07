﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtsWebClient.EtsHttpModels
{
   public class ProjectTasks
    {

        public string project_id { get; set; }
        public List<TaskTypes> task_types { get; set; }


    }

  public  class TaskTypes
    {
        public string id { get; set; }
        public string title { get; set; }
        public bool deleted { get; set; }
    }

}



