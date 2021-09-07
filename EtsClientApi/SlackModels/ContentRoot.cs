using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtsClientApi.SlackModels
{
    public class ContentRoot
    {
        public string Token { get; set; }
        public string Challenge { get; set; }
        public string Type { get; set; }

        public EventClass Event { get; set; }


    }
}
