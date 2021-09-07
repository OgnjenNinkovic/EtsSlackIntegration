using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebClientDemo.Models
{
    class TimeUnit
    {
        public string employee_id { get; set; }
        public string project_id { get; set; }

        public string task_type_id { get; set; }

        public ushort minutes { get; set; }

        public string description { get; set; }

        public string date { get; set; }

        public bool overtime { get; set; }

        public string next_time_unit_id { get; set; }

    }
}
