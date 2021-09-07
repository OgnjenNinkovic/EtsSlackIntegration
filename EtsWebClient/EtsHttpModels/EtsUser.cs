using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtsWebClient.EtsHttpModels
{
    public class EtsUser
    {
        public static EtsUser userData = new EtsUser();
        public string password { get; private set; }
        public string slcakChannelID { get; set; }
        private string m_username;
        public string username { set {
                m_username = value;
            }
            get
            {
                return @"UA\"+m_username;
            }
        }
       


        public EtsUser()
        {

        }

        public EtsUser(string username, string password,string slackChannelId)
        {
            this.slcakChannelID = slackChannelId;
            this.username = username;
             this.password = password;
            userData.username = username;
            userData.password = password;
            userData.slcakChannelID = slackChannelId;
        }






  
    }
}
