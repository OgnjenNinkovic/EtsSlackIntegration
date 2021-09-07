using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtsClientApi.SlackModels
{
    public class TimeUnitCRUDForm
    {
        public object view_id { get; set; }
        public string channel { get; set; }
        public string trigger_id { get; set; }
        public TimeUnitCRUDFormView view { get; set; }

        public TimeUnitCRUDForm(string callbackId, string formTitle)
        {
            this.view = new TimeUnitCRUDFormView(callbackId, formTitle);
        }
    }

    public class TimeUnitCRUDFormView
    {
        public string type { get; set; }
        public string callback_id { get; set; }
        public TimeUnitCRUDFormTitle title { get; set; }
        public TimeUnitCRUDFormSubmit submit { get; set; }
        public TimeUnitCRUDFormClose close { get; set; }
        public List<TimeUnitCRUDFormBlock> blocks { get; set; }

        public TimeUnitCRUDFormView(string callbackId, string formTitle, string viewType = "modal")
        {
            this.title = new TimeUnitCRUDFormTitle(formTitle);
            this.submit = new TimeUnitCRUDFormSubmit();
            this.type = "modal";
            this.callback_id = callbackId;
            this.close = new TimeUnitCRUDFormClose();

            this.blocks = new List<TimeUnitCRUDFormBlock>();


        }
    }

    public class TimeUnitCRUDFormTitle
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }


        public TimeUnitCRUDFormTitle(string titleText)
        {
            this.type = "plain_text";
            this.text = titleText;
            this.emoji = true;
        }
    }

    public class TimeUnitCRUDFormSubmit
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }

        public TimeUnitCRUDFormSubmit()
        {
            this.type = "plain_text";
            this.text = "Submit";
            this.emoji = true;


        }
    }

    public class TimeUnitCRUDFormClose
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }


        public TimeUnitCRUDFormClose()
        {
            this.type = "plain_text";
            this.text = "Cancel";
            this.emoji = true;


        }
    }


    public abstract class TimeUnitCRUDFormBlock
    {
        public string type { get; set; }
    }

    public class TimeUnitCRUD_ActionSelector : TimeUnitCRUDFormBlock
    {
        public string block_id { get; set; }

        public TimeUnitCRUDFormText text { get; set; }
        public TimeUnitCRUDFormAccessory accessory { get; set; }

        public TimeUnitCRUD_ActionSelector()
        {
            this.block_id = "etsCrudFormActionSelector";
            this.type = "section";
            this.text = new TimeUnitCRUDFormText("mrkdwn", "*Select a time entry option.*");

            this.accessory = new TimeUnitCRUDFormAccessory("static_select", "etsCrudFormActionSelected", "Options");
        }
    }


    public class TimeUnitCRUD_Divider : TimeUnitCRUDFormBlock
    {
        public TimeUnitCRUD_Divider(string type = "divider")
        {
            this.type = type;
        }
    }


    public class TimeUnitCRUD_StaticSelect : TimeUnitCRUDFormBlock
    {


        public bool optional { get; set; }
        public bool dispatch_action { get; set; }
        public string block_id { get; set; }


        public TimeUnitCRUDFormElement_StaticSelect element { get; set; }


        public TimeUnitCRUDFormLabel label { get; set; }

        public TimeUnitCRUD_StaticSelect(bool dispatchAction, string blockId, string labelText, string placeHolderText, string actionId, string type = "input")
        {
            this.type = type;
            this.optional = false;
            this.dispatch_action = dispatchAction;
            this.block_id = blockId;
            this.label = new TimeUnitCRUDFormLabel(labelText);
            this.element = new TimeUnitCRUDFormElement_StaticSelect(placeHolderText, actionId);
        }
    }




    public class TimeUnitCRUD_MultilineText : TimeUnitCRUDFormBlock
    {


        public bool optional { get; set; }
        public bool dispatch_action { get; set; }
        public string block_id { get; set; }


        public TimeUnitCRUDFormElement_MultilineText element { get; set; }


        public TimeUnitCRUDFormLabel label { get; set; }

        public TimeUnitCRUD_MultilineText(bool dispatchAction, string blockId, string labelText, string placeHolderText, string actionId, string type = "input")
        {
            this.type = type;
            this.optional = false;
            this.dispatch_action = dispatchAction;
            this.block_id = blockId;
            this.label = new TimeUnitCRUDFormLabel(labelText);
            this.element = new TimeUnitCRUDFormElement_MultilineText(actionId, placeHolderText);
        }
    }




    public class TimeUnitCRUDFormElement_StaticSelect : TimeUnitCRUDFormElement
    {

        public string type { get; set; }
        public string action_id { get; set; }
        public List<FormElementOption> options { get; set; }

        public TimeUnitCRUDFormElement_StaticSelect(string placeHolderText, string actionID)
        {
            this.type = "static_select";
            this.placeholder = new FormElementPlaceholder(placeHolderText);
            this.options = new List<FormElementOption>();
            this.action_id = actionID;
        }

    }






    public class TimeUnitCRUDFormText
    {
        public string type { get; set; }
        public string text { get; set; }
        public TimeUnitCRUDFormText(string type, string text)
        {
            this.type = type;
            this.text = text;
        }
    }

    public class TimeUnitCRUDFormAccessory
    {
        public string type { get; set; }
        public TimeUnitCRUDFormPlaceholder placeholder { get; set; }
        public List<TimeUnitCRUDFormOption> options { get; set; }
        public string action_id { get; set; }

        public TimeUnitCRUDFormAccessory(string type, string actionId, string placeholderText)
        {
            this.type = type;
            this.action_id = actionId;
            this.placeholder = new TimeUnitCRUDFormPlaceholder(placeholderText);
            this.options = new List<TimeUnitCRUDFormOption>();
            this.options.Add(new TimeUnitCRUDFormOption("Create", "create"));
            this.options.Add(new TimeUnitCRUDFormOption("Modify", "update"));
            this.options.Add(new TimeUnitCRUDFormOption("Delete", "delete"));

        }
    }

    public class TimeUnitCRUDFormPlaceholder
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }

        public TimeUnitCRUDFormPlaceholder(string text)
        {
            this.type = "plain_text";
            this.emoji = true;
            this.text = text;
        }
    }

    public class TimeUnitCRUDFormOption
    {
        public FormOptionText text { get; set; }
        public string value { get; set; }

        public TimeUnitCRUDFormOption(string optionText, string optionValue)
        {
            this.text = new FormOptionText(optionText);
            this.value = optionValue;
        }
    }

    public class FormOptionText
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }

        public FormOptionText(string text)
        {
            this.type = "plain_text";
            this.text = text;
            this.emoji = true;
        }
    }



    public abstract class TimeUnitCRUDFormElement
    {

        public FormElementPlaceholder placeholder { get; set; }


    }




    public class TimeUnitCRUDFormElement_MultilineText : TimeUnitCRUDFormElement
    {
        public string action_id { get; set; }

        public bool multiline { get; set; }

        public string type { get; set; }
        public TimeUnitCRUDFormElement_MultilineText(string actionID, string placeholderText)
        {
            this.action_id = actionID;
            this.multiline = true;
            this.type = "plain_text_input";
            this.placeholder = new FormElementPlaceholder(placeholderText);
        }


    }





    public class FormElementPlaceholder
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }

        public FormElementPlaceholder(string text)
        {
            this.type = "plain_text";
            this.text = text;
            this.emoji = true;

        }
    }

    public class FormElementOption
    {
        public FormElementOptionText text { get; set; }
        public string value { get; set; }

        public FormElementOption(string text, string value)
        {
            this.text = new FormElementOptionText(text);
            this.value = value;
        }
    }

    public class FormElementOptionText
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }

        public FormElementOptionText(string text)
        {
            this.type = "plain_text";
            this.text = text;
            this.emoji = true;
        }
    }

    public class TimeUnitCRUDFormLabel
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }


        public TimeUnitCRUDFormLabel(string text)
        {
            this.type = "plain_text";
            this.text = text;
            this.emoji = true;
        }
    }

}
