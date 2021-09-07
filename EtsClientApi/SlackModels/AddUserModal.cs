using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtsClientApi.SlackModels
{
  public  class AddUserControl
    {
        [JsonProperty("view_id")]
        public object view_id { get; set; }

        [JsonProperty("channel")]
        public string channel { get; set; }

        [JsonProperty("trigger_id")]
        public string trigger_id { get; set; }

        public AddUser view { get; set; }

        public AddUserControl(AddUser rootobject)
        {
            this.view = rootobject;
        }

        public class AddUser
        {


            [JsonProperty("type")]
            public string type { get; set; }


            [JsonProperty("callback_id")]
            public string callback_id { get; set; }


            public AddUserTitle title { get; set; }
            public AddUserSubmit submit { get; set; }

            public AddUserClose close { get; set; }
            public List<AddUserBlock> blocks { get; set; }

            public AddUser(AddUserTitle title, AddUserSubmit submit, AddUserClose addUserClose, List<AddUserBlock> blocks)
            {
                this.title = title;
                this.submit = submit;
                this.blocks = blocks;
                this.close = addUserClose;

            }
        }

        public class AddUserTitle
        {
            public string type { get; set; }
            public string text { get; set; }

            public bool emoji { get; set; } = true;
        }

        public class AddUserSubmit
        {
            public string type { get; set; }
            public string text { get; set; }

            public bool emoji { get; set; } = true;
        }

        public class AddUserClose
        {
            public string type { get; set; }
            public string text { get; set; }

            public bool emoji { get; set; } = true;

            public AddUserClose(string type, string text)
            {
                this.type = type;
                this.text = text;
            }
        }

        public class AddUserInputBlock : AddUserBlock
        {
            public string block_id { get; set; }

            [JsonProperty("element")]
            public AddUserElement element { get; set; }
            public AddUserLabel label { get; set; }

            public AddUserInputBlock(AddUserElement element, AddUserLabel label)
            {
                this.element = element;
                this.label = label;
            }
        }



        public abstract class AddUserBlock
        {
            public string type { get; set; }
        }

        public class AddUserActionBlock : AddUserBlock
        {
            public List<AddUserActionBlock_Elements> elements { get; set; }
            public AddUserActionBlock()
            {
                this.type = "actions";
                this.elements = new List<AddUserActionBlock_Elements>();
                elements.Add(new AddUserActionBlock_Elements("danger", "Unsubcribe", "unsubBtn", "unsub_btn_click"));
            }
        }

        public class AddUserActionBlock_Elements
        {
            public string type { get; set; }
            public string style { get; set; }

            public ActionElementText text { get; set; }

            public string value { get; set; }

            public string action_id { get; set; }

            public AddUserActionBlock_Elements(string elemStyle, string elemText, string elemVal, string actionID)
            {
                this.type = "button";
                this.style = elemStyle;
                this.text = new ActionElementText(elemText);
                this.value = elemVal;
                this.action_id = actionID;
            }

        }

        public class ActionElementText
        {
            public string type { get; set; }
            public string text { get; set; }

            public bool emoji { get; set; }

            public ActionElementText(string elemText)
            {
                this.type = "plain_text";
                this.text = elemText;
                this.emoji = true;

            }
        }










        public class AddUserElement
        {

            [JsonProperty("type")]
            public string type { get; set; }

            [JsonProperty("action_id")]
            public string action_id { get; set; }
            public AddUserPlaceholder placeholder { get; set; }

            public AddUserElement(AddUserPlaceholder placeholder)
            {
                this.placeholder = placeholder;
            }

        }

        public class AddUserPlaceholder
        {
            public string type { get; set; }
            public string text { get; set; }

            public AddUserPlaceholder(string placeHolderType, string placeholderText)
            {
                this.type = placeHolderType;
                this.text = placeholderText;
            }
        }

        public class AddUserLabel
        {
            public string type { get; set; }
            public string text { get; set; }
            public bool emoji { get; set; }
        }

    }
}
