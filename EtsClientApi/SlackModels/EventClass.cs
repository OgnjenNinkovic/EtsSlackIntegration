using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtsClientApi.SlackModels
{
    public class EventClass
    {
        public string Type { get; set; }
        public string bot_id { get; set; }

        public string client_msg_id { get; set; }
        public string User { get; set; }

        public string Channel { get; set; }


        public  string Text { get; set; }
    }
}
