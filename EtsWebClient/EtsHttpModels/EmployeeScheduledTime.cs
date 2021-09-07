using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtsWebClient.EtsHttpModels
{
    class EmployeeScheduledTime
    {
        public string employee_id { get; set; }
        public string project_id { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string scheduled_hours { get; set; }
    }
}
