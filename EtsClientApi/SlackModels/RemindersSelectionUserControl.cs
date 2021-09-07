using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtsClientApi.SlackModels
{

  public  class RemindersSelection
    {
        public string ReminderType { get; set; }
        public string Date { get; set; }

        public string ReminderID { get; set; }
    }

   public class RemindersSelectionUserControl
    {

        [JsonProperty("view_id")]
        public string view_id { get; set; }


        [JsonProperty("channel")]
        public string channel { get; set; }

        public string trigger_id { get; set; }


        [JsonProperty("view")]
        public RemindersSelectionView view { get; set; }



        public RemindersSelectionUserControl(RemindersSelectionView block)
        {
            this.view = block;
        }
    }

    public abstract class RemindersSelectionView
    {
        [JsonProperty("callback_id")]
        public string Callback_id { get; set; }


        [JsonProperty("type")]
        public string Type { get; set; } = "modal";


        [JsonProperty("title")]
        public RemindersSelectionViewTitle Title { get; set; }


        [JsonProperty("blocks")]
        public List<RemindersSelectionBlockBase> Blocks { get; set; }


    }


    public class RemindersSelectionView_Submit : RemindersSelectionView
    {



        [JsonProperty("submit")]
        public RemindersSelectionViewSubmit Submit { get; set; }





        [JsonProperty("close")]
        public RemindersSelectionViewClose Close { get; set; }





        public RemindersSelectionView_Submit(List<RemindersSelectionBlockBase> blocks, RemindersSelectionViewTitle title, RemindersSelectionViewClose close, RemindersSelectionViewSubmit submit)
        {
            this.Blocks = blocks;
            this.Title = title;
            this.Submit = submit;
            this.Close = close;
        }
    }





    public class RemindersSelectionView_Ok : RemindersSelectionView
    {

        [JsonProperty("close")]
        public RemindersSelectionViewClose Close { get; set; }

        public RemindersSelectionView_Ok(List<RemindersSelectionBlockBase> blocks, RemindersSelectionViewTitle title, RemindersSelectionViewClose close)
        {
            this.Blocks = blocks;
            this.Title = title;
            this.Close = close;
        }
    }

    public class RemindersSelectionViewTitle
    {

        [JsonProperty("type")]
        public string Type { get; set; }


        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("emoji")]

        public bool Emoji { get; set; }

        public RemindersSelectionViewTitle(string type = "plain_text", string text = "Create new reminder", bool emoji = true)
        {
            this.Type = type;
            this.Text = text;
            this.Emoji = emoji;
        }
    }

    public class RemindersSelectionViewSubmit
    {

        [JsonProperty("type")]
        public string Type { get; set; }


        [JsonProperty("text")]
        public string Text { get; set; }


        [JsonProperty("emoji")]
        public bool Emoji { get; set; }


        public RemindersSelectionViewSubmit(string type = "plain_text", string text = "Submit", bool emoji = true)
        {
            this.Type = type;
            this.Text = text;
            this.Emoji = emoji;
        }

    }

    public class RemindersSelectionViewClose
    {
        [JsonProperty("type")]
        public string Type { get; set; }


        [JsonProperty("text")]
        public string Text { get; set; }


        [JsonProperty("emoji")]
        public bool Emoji { get; set; }


        public RemindersSelectionViewClose(string type = "plain_text", string text = "Cancel", bool emoji = true)
        {
            this.Type = type;
            this.Text = text;
            this.Emoji = emoji;
        }
    }

    public abstract class RemindersSelectionBlockBase
    {

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class RemindersSelectionBlock : RemindersSelectionBlockBase
    {



        [JsonProperty("block_id")]
        public string Block_id { get; set; }

        [JsonProperty("element")]
        public RadioButtonElement Element { get; set; }


        [JsonProperty("label")]
        public DeleteReminderViewLabel Label { get; set; }



        public RemindersSelectionBlock(RadioButtonElement elem, DeleteReminderViewLabel label, string blockType = "input")
        {
            this.Type = blockType;
            this.Element = elem;
            this.Label = label;

        }
    }



    public class RemindersSelectionHeaderBlock : RemindersSelectionBlockBase
    {

        [JsonProperty("text")]

        public RadioBtnText text { get; set; }

        public RemindersSelectionHeaderBlock(RadioBtnText radioBtnText, string type = "header")
        {
            this.Type = type;
            this.text = radioBtnText;
        }
    }

    public class RadioButtonElement
    {

        [JsonProperty("type")]
        public string Type { get; set; }


        [JsonProperty("options")]
        public List<RadioBtnElem> Options { get; set; }

        [JsonProperty("action_id")]
        public string Action_id { get; set; }



        public RadioButtonElement(List<RadioBtnElem> options, string actionId = "radioButtonSelection")
        {
            this.Options = options;
            this.Action_id = actionId;
        }
    }

    public class RadioBtnElem
    {

        [JsonProperty("text")]
        public RadioBtnText Text { get; set; }


        [JsonProperty("value")]
        public string Value { get; set; }


        public RadioBtnElem(RadioBtnText radioBtnText, string reminderId = "reminder id")
        {
            this.Text = radioBtnText;
            this.Value = reminderId;
        }
    }

    public class RadioBtnText
    {
        [JsonProperty("type")]
        public string Type { get; set; }


        [JsonProperty("text")]
        public string Text { get; set; }


        [JsonProperty("emoji")]
        public bool Emoji { get; set; }

        public RadioBtnText(string type = "plain_text", string text = "Reminder details", bool emoji = true)
        {
            this.Type = type;
            this.Text = text;
            this.Emoji = emoji;
        }
    }

    public class DeleteReminderViewLabel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("emoji")]
        public bool Emoji { get; set; }

        public DeleteReminderViewLabel(string type = "plain_text", string text = "Label", bool emoji = true)
        {
            this.Type = type;
            this.Text = text;
            this.Emoji = emoji;
        }
    }

}
