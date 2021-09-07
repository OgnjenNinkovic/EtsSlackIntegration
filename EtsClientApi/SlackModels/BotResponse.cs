using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtsClientApi.SlackModels
{

    public class BotResponse
    {
        public bool ok { get; set; }
        public ResponseViewData view { get; set; }
    }

    public class ResponseViewData
    {
        public string id { get; set; }
        public string team_id { get; set; }
        public string type { get; set; }
        
    }


}
