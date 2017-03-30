using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace steam_friendlist_crawler.api
{
    public class SteamFriendsApi
    {
        private readonly string baseUrl;
        private HttpClient m_http;

        public SteamFriendsApi(string steamkey)
        {
            this.baseUrl = $"http://api.steampowered.com/ISteamUser/GetFriendList/v0001/?key={steamkey}&steamid=";
            m_http = new HttpClient();
        }

        public async Task<User> GetFriends(ulong steamid)
        {
            // Executa chamada no servidor
            var response = await m_http.GetAsync(this.baseUrl + steamid).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new User
                {
                    id = steamid,
                    friends = new List<Friend>()
                };
            }
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            // Constrói objeto json com a resposta
            var root = JObject.Parse(data);
            if (root["friendslist"] != null && root["friendslist"]["friends"] != null)
            {
                List<Friend> friends = null;
                var tdata = root["friendslist"]["friends"].ToString();
                friends = JsonConvert.DeserializeObject<List<Friend>>(tdata);
                return new User
                {
                    id = steamid,
                    friends = friends
                };
            }
            else
            {
                return new User
                {
                    id = steamid,
                    friends = new List<Friend>()
                };
            }
        }
    }
}