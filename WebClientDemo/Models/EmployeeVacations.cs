using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebClientDemo.Models
{
   public class EmployeeVacations
    {
      
        public string employee_id { get; set; }

      

        public Vacation[] vacations { get; set; }
      
    }
}
