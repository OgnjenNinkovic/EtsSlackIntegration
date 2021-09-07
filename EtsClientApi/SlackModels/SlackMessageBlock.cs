using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EtsClientApi.SlackModels
{
    public class SlackMessageBlock
    {

        public string text { get; set; }

        public string channel { get; set; }


        public List<Attachment> attachments { get; set; }

        public static SlackMessageBlock createMessage(string header, string message, bool msgStatus, string slackChannel,string msgPreview= "")
        {
            string hexColor = msgStatus ? "#3c953d" : "#9e2041";
            var msgBlocks = new List<MsgBlock>();
            var Header = new MsgBlock("header", text: $"{header}");
            msgBlocks.Add(Header);
            var userNotification = new MsgBlock("section", "mrkdwn", $"*{message}*.");
            msgBlocks.Add(userNotification);


            Attachment attachment = new Attachment(msgBlocks);
            attachment.color = $"{hexColor}";


            string msgPrew = string.IsNullOrEmpty(msgPreview) ? message : msgPreview;
          

            SlackMessageBlock slackMessage = new SlackMessageBlock(attachment, msgPrew);
            slackMessage.channel = $"{slackChannel}";

            return slackMessage;
        }

        public SlackMessageBlock(Attachment msgAttachments,string msgPreview)
        {
            this.text = msgPreview;
            List<Attachment> newAttachment = new List<Attachment>();
            newAttachment.Add(msgAttachments);


            this.attachments = newAttachment;

        }


    }



    public class Attachment
    {
        public string color { get; set; }
        public List<MsgBlock> blocks { get; set; }

        public Attachment(List<MsgBlock> blocks = null)
        {
            if (blocks == null)
            {
                MsgBlock block = new MsgBlock();
                blocks = new List<MsgBlock>();
                blocks.Add(block);

                this.blocks = blocks;
            }
            else
            {
                this.blocks = blocks;
            }


        }
    }

    public class MsgBlock
    {
        public string type { get; set; }
        public MsgText text { get; set; }


        public MsgBlock(string blockType = "section", string textType = "plain_text", string text = "")
        {
            MsgText initial = new MsgText();
            initial.type = textType;
            initial.text = text;
            this.text = initial;
            this.type = blockType;


        }
    }

    public class MsgText
    {
        public string type { get; set; }
        public string text { get; set; }


    }



   

}
