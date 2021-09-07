using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static EtsClientData.EtsDataContext;

namespace EtsClientApi.SlackModels
{
    public class ControlConfigurator
    {
        public bool isUpdate =false;


        public bool showDatepicker = false;

        public bool showWeekdaysStaticSelect = false;

        public enum controlInterface { createNewReminder, updateReminder };

        public string CallBackId { get; set; }
        public controlInterface ControlInterface { get; set; }

        public string DialogTitle { get; set; }

        public string BtnPrimaryLabel { get; set; }

        public string BtnDeleteLabel { get; set; }

        public int InitilNumOfAddTimeControl { get; set; }


        public string ReminderTypePlaceholderText { get; set; }


        public string ReminderDatePlaceholder { get; set; }

        public string WeekdayPlaceholder { get; set; }

        public List<ReminderTime> ReminderTimes { get; set; }



        public ControlConfigurator(controlInterface controlInterface)
        {
            this.ControlInterface = controlInterface;
            ReminderTimes = new List<ReminderTime>();
            switch (controlInterface)
            {
                case controlInterface.createNewReminder:
                    this.DialogTitle = "Create new reminder";
                    this.BtnPrimaryLabel = "Add time";
                    this.InitilNumOfAddTimeControl = 1;
                    this.ReminderTypePlaceholderText = "Select a reminder type";
                    this.ReminderDatePlaceholder = "Select a date";
                    this.CallBackId = "createNewReminder";
                    this.WeekdayPlaceholder = "Select a day in week";
                    break;
                case controlInterface.updateReminder:
                    this.isUpdate = true;
                    this.DialogTitle = "Modify the reminder";
                    this.BtnPrimaryLabel = "Add time";
                    this.ReminderTypePlaceholderText = "Set the reminder type";
                    this.ReminderDatePlaceholder = "Set the reminder date ";
                    this.WeekdayPlaceholder = "Select a day in week";
                    this.InitilNumOfAddTimeControl = ReminderTimes.Count;
                    this.InitilNumOfAddTimeControl++;
                    this.BtnDeleteLabel = "Delete time";
                    this.ReminderTimes = null;
                    this.CallBackId = "updateReminder_submit";
                    break;

            }
        }




    }

   public class CreateNewReminder
    {

        [JsonProperty("view_id")]
        public string view_id { get; set; }

        [JsonProperty("channel")]
        public string channel { get; set; }

        public string trigger_id { get; set; }


        public AddReminderControll view { get; set; }

        public CreateNewReminder()
        {

        }
       

        public CreateNewReminder(AddReminderControll block)
        {
            this.view = block;
        }
        public class BlockPlaceholder
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }

           
          

            public BlockPlaceholder(string placeholderText="Placeholder text template",string placeholderType = "plain_text")
            {
                this.Type = placeholderType;
                this.Text = placeholderText;

            }


        }

        public class BlockText
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }

            public BlockText(string type = "plain_text", string staticOptionText ="Static select")
            {
                this.Type = type;
                this.Text = staticOptionText;
            }

        }

        public class BlockOption
        {
            [JsonProperty("text")]
            public BlockText Text { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }

            public BlockOption(BlockText text )
            {
                this.Text = text;
                this.Value = text.Text;
            }
        }

        public abstract class BlockElement
        {


            [JsonProperty("type")]
            public string Type { get; set; }



            [JsonProperty("action_id")]
            public string ActionId { get; set; }





        }


        public class StaticSelect : BlockElement
        {
            [JsonProperty("placeholder")]
            public BlockPlaceholder Placeholder { get; set; }

            [JsonProperty("options")]
            public List<BlockOption> Options { get; set; }

         
            public StaticSelect(BlockPlaceholder placeholder, List<BlockOption> options)
            {
                this.Placeholder = placeholder;

                this.Options = options;
            }
        }

        public class DatePicker : BlockElement
        {
            [JsonProperty("type")]
            public string ElementType { get; set; }



            [JsonProperty("placeholder")]
            public BlockPlaceholder Dplaceholder { get; set; }

            public DatePicker(BlockPlaceholder placeholder)
            {
                this.Dplaceholder = placeholder;
            }

        }

        public class Timepicker : BlockElement
        {
            [JsonProperty("type")]
            public string ElementType { get; set; }

            [JsonProperty("placeholder")]
            public BlockPlaceholder Tplaceholder { get; set; }

            public Timepicker()
            {

            }
            public Timepicker(BlockPlaceholder blockPlaceholder, string Type = "timepicker", string action_id = "actionId_3")
            {
                this.ElementType = Type;
                this.ActionId = action_id;
                this.Tplaceholder = blockPlaceholder;
            }
        }


     
        public class Button : BlockElement
        {



            [JsonProperty("type")]
            public string ElementType { get; set; }


            [JsonProperty("style")]
            public string Style { get; set; }

            [JsonProperty("value")]
            public string BtnValue { get; set; }

            [JsonProperty("text")]
            public BlockText Text { get; set; }



            public Button(BlockText text)
            {
                this.Text = text;
            }
        }

        public class ControllBlock
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("block_id")]
            public string BlockId { get; set; }

            [JsonProperty("elements")]
            public List<BlockElement> Elements { get; set; }


            public ControllBlock(List<BlockElement> elements)
            {
                this.Elements = elements;
            }
        }

        public class AddReminderControll
        {
            public string type { get; set; }

            [JsonProperty("callback_id")]
            public string callback_id { get; set; }
            public AddReminderTitle title { get; set; }

            public AddReminderSubmitBtn submit { get; set; }

            public AddReminderCloseBtn close { get; set; }



            [JsonProperty("blocks")]
            public List<ControllBlock> Blocks { get; set; }

            public AddReminderControll(List<ControllBlock> blocks, AddReminderTitle addReminderTitle, AddReminderSubmitBtn submitBtn, AddReminderCloseBtn closeBtn)
            {

                this.Blocks = blocks;
                this.title = addReminderTitle;
                this.submit = submitBtn;
                this.close = closeBtn;
            }
        }


        public class AddReminderTitle
        {
            public string type { get; set; }
            public string text { get; set; }

            public AddReminderTitle(string type = "plain_text", string text = "Create new reminder")
            {
                this.type = type;
                this.text = text;
            }
        }


        public class AddReminderSubmitBtn
        {
            public string type { get; set; }
            public string text { get; set; }

            public bool emoji { get; set; }

            public AddReminderSubmitBtn(string type = "plain_text", string text = "Submit", bool emoji = true)
            {
                this.type = type;
                this.text = text;
                this.emoji = emoji;
            }
        }

        public class AddReminderCloseBtn
        {
            public string type { get; set; }
            public string text { get; set; }

            public bool emoji { get; set; }

            public AddReminderCloseBtn(string type = "plain_text", string text = "Cancel", bool emoji = true)
            {
                this.type = type;
                this.text = text;
                this.emoji = emoji;
            }
        }


    }




}
