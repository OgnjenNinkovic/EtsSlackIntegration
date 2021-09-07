using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebClientDemo.Models
{
    class EtsTimeUnitData
    {
        public string id { get; set; } 
        public string employee_id { get; set; } 
        public short minutes { get; set; }
        public string date { get; set; }
        public string description { get; set; } 
        public string project_id { get; set; }
        public string project_title { get; set; }
        public bool overtime { get; set; }
        public string status { get; set; }
        public string task_type_id { get; set; }
        public string task_type_title { get; set; }
        public bool is_time_unit_locked { get; set; }
        public bool is_billable { get; set; }
        public string is_overtime_payable { get; set; }
        public string overtime_multiplier { get; set; }
        public string source_id { get; set; }
        public string source_name { get; set; }
        public string next_time_unit_id { get; set; }
    }
}
