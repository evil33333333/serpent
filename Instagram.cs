using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Serpent
{
    internal class Instagram
    {

        public static string LoginWithUsernameAndPassword(string username, string password)
        {
            using (HttpClient httpClient = new HttpClient())
            {

                httpClient.DefaultRequestHeaders.Add("x-csrftoken", "JTgWH0GhGYX6pqKQkpkjEIvoRFEjkUIc");
                httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.41 Safari/537.36 Edg/101.0.1210.32");

                using (StringContent content = new StringContent($"username={username}&enc_password=#PWD_INSTAGRAM_BROWSER:0:0:{password}&optIntoOneTap=false&queryParams=%7B%7D"))
                {
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
                    using (HttpResponseMessage message = httpClient.PostAsync("https://instagram.com/accounts/login/ajax/", content).Result)
                    {
                        if (message.IsSuccessStatusCode)
                        {
                            foreach (string value in message.Headers.GetValues("Set-Cookie"))
                            {
                                if (value.Contains("sessionid"))
                                {
                                    string session_id = value.Split(new string[] { "sessionid=" }, StringSplitOptions.None)[1].Split(';')[0];
                                    return session_id;
                                }
                            }
                            return null;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
                
        }
        public static List<String> GetBannedUsers(string session_id)
        {
            String response = Instagram.GetSupportRequestResponse(session_id);
            if (response == null)
                return null;
            List<String> banned_users = Instagram.ParseResponse(response);
            return banned_users;
        }

        private static String GetSupportRequestResponse(string session_id)
        {
            String response = null;
            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.124 Safari/537.36 Edg/102.0.1245.41");
                webClient.Headers.Add("X-CSRFTOKEN", "KYvt6teafDoiGQRDpoiRKixfwWASIFQ7");
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                webClient.Headers.Add("Cookie", $"sessionid={session_id}");
                try
                {
                    response = webClient.UploadString("https://i.instagram.com/api/v1/bloks/apps/com.instagram.support.support_reports/", "tab_bar_surface=3&nest_data_manifest=true");
                }
                catch
                {
                    
                }
                return response;
            }
        }

        private static List<String> ParseResponse(string responseString)
        {
            try
            {
                List<String> result = new List<String>();
                JObject responseObject = JsonConvert.DeserializeObject<JObject>(responseString);
                JArray array = responseObject.SelectToken("$.['layout']['bloks_payload']['tree']['bk.components.Flexbox']['children'][1]['bk.components.Flexbox']['children'][0]['bk.components.Collection']['children'][0]['bk.components.Flexbox']['children']").ToObject<JArray>();
                foreach (JToken token in array)
                {
                    string target = token.SelectToken("$.['bk.components.Flexbox']['children'][0]['bk.components.Flexbox']['children'][1]['bk.components.Flexbox']['children'][0]['bk.components.Flexbox']['children'][0]['bk.components.RichText']['children'][0]['bk.components.TextSpan']['text']").ToObject<String>();
                    string support_response = token.SelectToken("$.['bk.components.Flexbox']['children'][0]['bk.components.Flexbox']['children'][1]['bk.components.Flexbox']['children'][0]['bk.components.Flexbox']['children'][1]['bk.components.RichText']['children'][0]['bk.components.TextSpan']['text']").ToObject<String>();
                    if (support_response == "Closed")
                    {
                        result.Add(target);
                    }
                }

                return result;
            }
            catch
            {
                return null;
            }
            
        }

        public static bool IsSession(string account)
        {
            return !account.Contains(":");
        }
    }
}
