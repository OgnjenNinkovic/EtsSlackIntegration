using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtsClientApi.SlackModels
{

    public class InfoPopUp
    {
        public object view_id { get; set; }
        public string channel { get; set; }
        public string trigger_id { get; set; }
        public InfoView view { get; set; }

        public InfoPopUp(string title = "", string neutralBtnTxt = "", string headerBlockText = "")
        {

            this.view = new InfoViewNeutral(title, neutralBtnTxt, headerBlockText);

        }

        public InfoPopUp(string submitBtnTxt, string callBackID, string title = "", string neutralBtnTxt = "", string headerBlockText = "")
        {
            this.view = new InfoViewSubmit(title, neutralBtnTxt, submitBtnTxt, headerBlockText, callBackID);

        }

    }


    public abstract class InfoView
    {

        public string type { get; set; }
        public string callback_id { get; set; }

        public InfoViewTitle title { get; set; }


    }


    class InfoViewNeutral : InfoView
    {

        public InfoClose close { get; set; }
        public List<InfoViewBlock> blocks { get; set; }

        public InfoViewNeutral(string viewTitle, string dialogBtnTxt, string headerBlockText)
        {
            this.type = "modal";

            this.title = new InfoViewTitle();
            title.type = "plain_text";
            title.emoji = true;
            title.text = viewTitle;



            this.close = new InfoClose();
            close.type = "plain_text";
            close.text = dialogBtnTxt;
            close.emoji = true;

            this.callback_id = "";


            InfoViewBlock headerBlock = new InfoViewBlock(headerBlockText);


            this.blocks = new List<InfoViewBlock>();
            blocks.Add(headerBlock);

        }
    }


    class InfoViewSubmit : InfoView
    {
        public InfoSubmit submit { get; set; }
        public InfoClose close { get; set; }
        public List<InfoViewBlock> blocks { get; set; }


        public InfoViewSubmit(string viewTitle, string neutralBtnTxt, string submitBtnTxt, string headerBlockText, string callBackId)
        {
            this.type = "modal";

            this.title = new InfoViewTitle();
            title.type = "plain_text";
            title.emoji = true;
            title.text = viewTitle;

            this.submit = new InfoSubmit();
            submit.type = "plain_text";
            submit.text = submitBtnTxt;
            submit.emoji = true;


            this.close = new InfoClose();
            close.type = "plain_text";
            close.text = neutralBtnTxt;
            close.emoji = true;

            this.callback_id = callBackId;


            InfoViewBlock headerBlock = new InfoViewBlock(headerBlockText);


            this.blocks = new List<InfoViewBlock>();
            blocks.Add(headerBlock);
        }
    }





    public class InfoViewTitle
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }


    }

    public class InfoClose
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }

    }

    public class InfoSubmit
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }

    }

    public class InfoViewBlock
    {
        public string type { get; set; }
        public InfoViewText text { get; set; }

        public InfoViewBlock(string headerBlockText)
        {
            this.type = "header";
            this.text = new InfoViewText();
            text.text = headerBlockText;
            text.type = "plain_text";
            text.emoji = true;
        }
    }

    public class InfoViewText
    {
        public string type { get; set; }
        public string text { get; set; }
        public bool emoji { get; set; }
    }



}
