using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace sharedrasil {
    public static class Github {
        static string baseUrl = "https://api.github.com";

        public async static Task GetAll() {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", Globals.currentUser.AccessToken);
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

            string jsonString = await client.GetStringAsync(baseUrl);

            JsonConvert.DeserializeObject(jsonString);
        }
    }
}