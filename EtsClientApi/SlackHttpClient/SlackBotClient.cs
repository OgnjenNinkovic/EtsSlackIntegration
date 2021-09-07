using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EtsClientApi.Api;
using EtsClientApi.SlackModels;
using Newtonsoft.Json;

namespace EtsClientApi.SlackHttpClient
{
    public class SlackBotClient
    {
        private Uri _url;
        private readonly HttpClient _client;

        public enum PostUriType {postMessage, openViews, updateViews,userInfo , botHomeTab};

        public string SlackUserID { get; set; }
      

        public SlackBotClient(string token, string slackUserId = "")
        {



            this.SlackUserID = slackUserId;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task WriteMessage(object jsonPayload = null, PostUriType postUriType = PostUriType.postMessage )
        {
            var json = JsonConvert.SerializeObject(jsonPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            switch (postUriType)
            {
                case PostUriType.postMessage:
                   _url = SlackBotApi.PostMessage;
                   var postMsgResult =  await _client.PostAsync(_url, content);
                    var postMsgResponse = await postMsgResult.Content.ReadAsStringAsync();
                    break;
                case PostUriType.openViews:
                    _url = SlackBotApi.ViewesOpen;

                     var openResult = await _client.PostAsync(_url, content);
                    var responseContent =await openResult.Content.ReadAsStringAsync();
                    SlackController.botResponse = JsonConvert.DeserializeObject<BotResponse>(responseContent);

                    break;
                case PostUriType.updateViews:
                    _url = SlackBotApi.ViewsUpdate;

                    var updateResult = await _client.PostAsync(_url, content);
                    var responseUpdate = await updateResult.Content.ReadAsStringAsync();
                    SlackController.botResponse = JsonConvert.DeserializeObject<BotResponse>(responseUpdate);
                    break;
                case PostUriType.userInfo:
                    SlackBotApi.slackUserId = this.SlackUserID;
                    _url = SlackBotApi.UserInfo;

                    var userInfoResult = await _client.PostAsync(_url, content);

                    var responseUserInfo = await userInfoResult.Content.ReadAsStringAsync();

                    SlackController.UserProfile = JsonConvert.DeserializeObject<UserProfile>(responseUserInfo);

                    break;
                case PostUriType.botHomeTab:
                    _url = SlackBotApi.BotHomeTab;

                    var botHomeTabResult = await _client.PostAsync(_url, content);
                    var botHomeTabResponse = await botHomeTabResult.Content.ReadAsStringAsync();

                    break;
            }

      
            
        }
    }
}
