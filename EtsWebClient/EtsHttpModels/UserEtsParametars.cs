using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtsWebClient.EtsHttpModels
{
   internal class UserEtsParametars
    {
        public string access_token { get; set; }

        public string refresh_token { get; set; }

        public EmployeeDetails employee_details { get; set; } 
    }
}
