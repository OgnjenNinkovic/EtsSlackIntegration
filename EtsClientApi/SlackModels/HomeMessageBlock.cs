using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtsClientApi.SlackModels
{

    public class HomeMessageBlock
    {
        public string user_id { get; set; }
        public HomeView view { get; set; }

        public HomeMessageBlock(string userId, string headerText, string btnLabel, string mainMessage, string btnClickID)
        {
            this.user_id = userId;

            this.view = new HomeView(headerText, btnLabel, mainMessage, btnClickID);
        }

    }

    public class HomeView
    {
        public string type { get; set; }
        public List<HomeViewBlock> blocks { get; set; }

        public HomeView(string headerText, string btnLabel, string mainMessage, string btnActionID)
        {
            this.type = "home";
            this.blocks = new List<HomeViewBlock>();

            blocks.Add(new HomeViewBlockStatic("header", new HomeBlockText_WithEmoji("plain_text", headerText)));
            blocks.Add(new HomeViewBlockStatic("section", new HomeBlockText_NoEmoji("mrkdwn", mainMessage)));
            blocks.Add(new HomeViewActionBlock(btnLabel, "btn_click", btnActionID));

        }


    }

    public abstract class HomeBlockText
    {
        public string type { get; set; }
        public string text { get; set; }
    }


    public class HomeBlockText_WithEmoji : HomeBlockText
    {

        public bool emoji { get; set; }

        public HomeBlockText_WithEmoji(string type, string text)
        {
            this.emoji = true;
            this.type = type;
            this.text = text;
        }
    }


    public class HomeBlockText_NoEmoji : HomeBlockText
    {

        public HomeBlockText_NoEmoji(string type, string text)
        {
            this.type = type;
            this.text = text;
        }
    }


    public abstract class HomeViewBlock
    {
        public string type { get; set; }

    }


    public class HomeViewBlockStatic : HomeViewBlock
    {

        public HomeBlockText text { get; set; }
        public HomeViewBlockStatic(string blockType, HomeBlockText text)
        {
            this.type = blockType;
            this.text = text;
        }


    }






    public class HomeViewActionBlock : HomeViewBlock
    {
        public List<HomeBlockBtnElement> elements { get; set; }

        public HomeViewActionBlock(string btnLabel, string actionVal, string actionID)
        {
            this.type = "actions";
            this.elements = new List<HomeBlockBtnElement>();
            elements.Add(new HomeBlockBtnElement(btnLabel, actionVal, actionID));

        }

    }

    public class HomeBlockBtnElement
    {
        public string style { get; set; }
        public string type { get; set; }
        public HomeBlockText text { get; set; }

        public string value { get; set; }
        public string action_id { get; set; }

        public HomeBlockBtnElement(string btnLabel, string actionVal, string actionId)
        {
            this.style = "primary";
            this.type = "button";
            this.text = new HomeBlockText_WithEmoji("plain_text", btnLabel);
            this.value = actionVal;
            this.action_id = actionId;
        }
    }

}
