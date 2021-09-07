using System;
using System.Collections.Generic;
using System.Text;

namespace EtsWebClient.Test
{
    class SlackInteractiveComponent
    {

        public class Rootobject
        {
            public Block[] blocks { get; set; }
        }

        public class Block
        {
            public string type { get; set; }
            public string block_id { get; set; }
            public Element[] elements { get; set; }
        }

        public class Element
        {
            public string type { get; set; }
            public Placeholder placeholder { get; set; }
            public string action_id { get; set; }
            public Option[] options { get; set; }
            public string initial_date { get; set; }
            public Text text { get; set; }
            public string value { get; set; }
        }

        public class Placeholder
        {
            public string type { get; set; }
            public string text { get; set; }
            public bool emoji { get; set; }
        }

        public class Text
        {
            public string type { get; set; }
            public string text { get; set; }
            public bool emoji { get; set; }
        }

        public class Option
        {
            public Text1 text { get; set; }
            public string value { get; set; }
        }

        public class Text1
        {
            public string type { get; set; }
            public string text { get; set; }
        }


    }
}
