using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EtsClientApi.SlackHttpClient
{
    internal static class HttpExtensions
    {
        public static Uri AddQuery(this Uri uri, string name, string value)
        {
            var httpValueCollection = HttpUtility.ParseQueryString(uri.Query);

            httpValueCollection.Remove(name);
            httpValueCollection.Add(name, value);

            var ub = new UriBuilder(uri);
            ub.Query = httpValueCollection.ToString();

            return ub.Uri;
        }





    }


    public class SlackBotApi
    {

        

      
        private static Uri root = new Uri("https://slack.com/api/");

        public static string slackUserId = string.Empty;

        public static Uri UserInfo
        {

            get
            {
                return new Uri(root, "users.info?")
                    .AddQuery("user",slackUserId);
            }
            
        }
    

        public static Uri PostMessage
        {
            get
            {
                return new Uri(root, "chat.postMessage");
            }
        }

        public static Uri ViewesOpen
        {
            get
            {
                return new Uri(root, "views.open");
             
            }
        }



        public static Uri ViewsUpdate
        {
            get
            {
                return new Uri(root, "views.update");
            }
        }


      

        public static Uri BotHomeTab
        {
            get
            {
                return new Uri(root, "views.publish");
            }
        }

    }
}
