using System;
using System.Collections.Generic;
using System.Text;

namespace EtsWebClient.SlackModels
{
    public class SlackBotNotification
    {
        public string text { get; set; }
        public string channel { get; set; }


        public List<BotNotificationAttachment> attachments { get; set; }


        public SlackBotNotification(BotNotificationAttachment botNotificationAttachment,string messagePreview ,string hexColor = "#3c953d")
        {
            var botNotificationAttachments = new List<BotNotificationAttachment>();
            botNotificationAttachment.color = hexColor;
            botNotificationAttachments.Add(botNotificationAttachment);

            this.attachments = botNotificationAttachments;
            this.text = messagePreview.Replace("*","");
        }
    }





    public class BotNotificationAttachment
    {
        public string color { get; set; }
        public List<NotificationBlock> blocks { get; set; }

        public BotNotificationAttachment(List<NotificationBlock> blocks)
        {
            this.blocks = blocks;
        }
    }


    public abstract class NotificationBlock
    {

    }

    public class HeaderBlock : NotificationBlock
    {
        public string type { get; set; }
        public NotificationBlockTxt text { get; set; }

        public HeaderBlock(string type = "header", string blockText = "Block text", string blockTextType = "plain_text")
        {
            NotificationBlockTxt notTxt = new NotificationBlockTxt();
            notTxt.text = blockText;
            notTxt.type = blockTextType;
            this.type = type;
            this.text = notTxt;
        }
    }


    public class AccessoryBlock : NotificationBlock
    {
        public string type { get; set; } = "section";
        public NotificationBlockTxt text { get; set; }
        public Accessory accessory { get; set; }

        public AccessoryBlock(Accessory accessory, string text = "Accessory block text")
        {
            NotificationBlockTxt notTxt = new NotificationBlockTxt();
            notTxt.type = "mrkdwn";
            notTxt.text = text;
            this.accessory = accessory;
            this.text = notTxt;
        }

    }


    public class NotificationBlockTxt
    {
        public string type { get; set; }
        public string text { get; set; }


        public NotificationBlockTxt(string type = "plain_text")
        {
            this.type = type;

        }
    }

    public class Accessory
    {
        public string type { get; set; }
        public string style { get; set; } = "primary";
        public NotificationBlockTxt text { get; set; }
        public string value { get; set; }
        public string action_id { get; set; }

        public Accessory(string type = "button", string value = "click_me_123", string actionId = "button-action", string btnLabel = "Button label")
        {
            var txt = new NotificationBlockTxt();
            txt.text = btnLabel;
            this.type = type;
            text = txt;
            this.value = value;
            this.action_id = actionId;
        }

    }


}
