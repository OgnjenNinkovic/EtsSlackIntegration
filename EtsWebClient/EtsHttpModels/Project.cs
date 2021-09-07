using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtsWebClient.EtsHttpModels
{
    public class Project
    {
        public string id { get; set; }
        public string title { get; set; }
        public List<Office> office { get; set; }

        public string status { get; set; }
        public string type { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string client_id { get; set; }
        public string client_name { get; set; }
    }
    public class Office
    {
        public string id { get; set; }
        public string title { get; set; }

    }


}
