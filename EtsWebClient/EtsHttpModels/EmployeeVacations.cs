using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtsWebClient.EtsHttpModels
{
   public class EmployeeVacations
    {
        [JsonProperty("employee_id")]
        public string employee_id { get; set; }


        [JsonProperty("vacations")]
        public  List<Vacation> vacations { get; set; }
      
    }

    public class Vacation
    {
        [JsonProperty("start_date")]
        public string StartDate { get; set; }

        [JsonProperty("end_date")]
        public string EndDate { get; set; }
    }
}
