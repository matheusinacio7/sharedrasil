using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace sharedrasil {

    public class GithubCodeResponse {
        public string device_code {get; set;}
        public string user_code {get; set;}
        public string verification_uri {get; set;}
        public int expires_in {get; set;}
        public int interval {get; set;}
    }

    public class GithubTokenResponse {
        public string access_token {get; set;}
        public string token_type {get; set;}
        public string scope {get; set;}
    }


    public static class Github {
        static string baseUrl = "https://api.github.com";
        static string loginUrl = "https://github.com/login";

        static string authUrl = $"{loginUrl}/oauth/access_token";
        static string codeUrl = $"{loginUrl}/device/code";

        public async static Task<Token> Authenticate() {
            Console.WriteLine("\nYou will receive a code, which you must enter in your browser in order to authorize sharedrasil to manage repositories on your behalf.");

            HttpClient client = new HttpClient();

            string content = JsonConvert.SerializeObject(new {
                client_id = Environment.GetEnvironmentVariable("CLIENT_ID")
            });

            client.DefaultRequestHeaders.Add("accept", "application/json");

            StringContent stringContent = new StringContent(content, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(codeUrl, stringContent);

            string body = await response.Content.ReadAsStringAsync();
            GithubCodeResponse authJson = JsonConvert.DeserializeObject<GithubCodeResponse>(body);

            Console.WriteLine($"Your code is {authJson.user_code}.");

            
            Console.WriteLine("Press any key to open your browser");
            Console.ReadKey(true);
            System.Diagnostics.Process.Start("explorer.exe", authJson.verification_uri);

            Console.WriteLine("Waiting for validation...");
            Console.WriteLine("Please wait up to 10 seconds after validating.");
            GithubTokenResponse tokenJson = AwaitForValidation(client, authJson).Result;

            Token token = new Token{
                Type = tokenJson.token_type,
                AccessToken = tokenJson.access_token,
            };

            return token;
        }

        public static async Task<GithubTokenResponse> AwaitForValidation(HttpClient client, GithubCodeResponse authJson) {
            do {
                await Task.Delay(authJson.interval * 1100);

                string content = JsonConvert.SerializeObject(new
                {
                    client_id = Environment.GetEnvironmentVariable("CLIENT_ID"),
                    device_code = authJson.device_code,
                    grant_type = "urn:ietf:params:oauth:grant-type:device_code",
                });
                StringContent stringContent = new StringContent(content, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(authUrl, stringContent);
                string body = await response.Content.ReadAsStringAsync();
                GithubTokenResponse tokenJson = JsonConvert.DeserializeObject<GithubTokenResponse>(body);

                if(!String.IsNullOrEmpty(tokenJson.access_token)) {
                    return tokenJson;
                }
            } while (true);
        }

        public static async Task Root() {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            client.DefaultRequestHeaders.Add("Authorization", "bearer f29bd168c7ca907feee510a82d487ff593a48e34");
            client.DefaultRequestHeaders.Add("User-Agent", "sharedrasil");

            HttpResponseMessage response = await client.GetAsync(baseUrl);
            string body = await response.Content.ReadAsStringAsync();
            JsonConvert.DeserializeObject(body);
        }
    }
}