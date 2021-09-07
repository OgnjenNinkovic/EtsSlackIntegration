using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EtsWebClient.Http
{
    public class SlackBotClient
    {
        private string _url;
        private readonly HttpClient _client;

        public SlackBotClient(string token)
        {




            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task WriteMessage(object jsonPayload, bool isModal = false)
        {
            string messageChannel = "https://slack.com/api/chat.postMessage";
            string modalChannel = "https://slack.com/api/views.open";

            var json = JsonConvert.SerializeObject(jsonPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _url = isModal ? modalChannel : messageChannel;
             var result = await _client.PostAsync(_url, content);
            var response = await result.Content.ReadAsStringAsync();


        }
    }
}
