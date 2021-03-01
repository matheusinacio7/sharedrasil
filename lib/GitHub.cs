using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;
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

        static string userUrl = "https://api.github.com/user";
        static string repoUrl = $"{baseUrl}/repos/{Globals.currentUser.Username}/{Globals.currentUser.Username}-worlds";

        static string authUrl = $"{loginUrl}/oauth/access_token";
        static string codeUrl = $"{loginUrl}/device/code";

        public async static Task<AccessToken> Authenticate() {
            Console.WriteLine("\nYou will receive a code, which you must enter in your browser in order to authorize sharedrasil to manage repositories on your behalf.");

            HttpClient client = new HttpClient();

            string content = JsonConvert.SerializeObject(new {
                client_id = Environment.GetEnvironmentVariable("CLIENT_ID"),
                scope = "repo",
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

            AccessToken token = new AccessToken{
                Type = tokenJson.token_type,
                Token = tokenJson.access_token,
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

        public static HttpClient CreateBaseClient() {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            client.DefaultRequestHeaders.Add("Authorization", $"token {Globals.currentUser.AccessToken.Token}");
            client.DefaultRequestHeaders.Add("User-Agent", "sharedrasil");

            return client;
        }

        public static async Task<Boolean> CheckIfExists(HttpClient client = null) {
            if(client is null)
                client = CreateBaseClient();
            
            HttpResponseMessage response = await client.GetAsync(repoUrl);

            Console.Write(response.StatusCode);

            HttpStatusCode statusCode = response.StatusCode;

            return !(statusCode == HttpStatusCode.NotFound);
        } 

        public static async Task CheckPermissions(HttpClient client = null) {
            if(client is null)
                client = CreateBaseClient();

            HttpResponseMessage response = await client.GetAsync($"https://api.github.com/users/{Globals.currentUser.Username}");
            Console.Write(response.Headers.ToString());
        }

        public static async Task Create(HttpClient client = null) {
            if(client is null)
                client = CreateBaseClient();

            if(await CheckIfExists(client)) {
                Console.WriteLine("\nThe remote repository already exists.");
            }

            string content = JsonConvert.SerializeObject(new
            {
                name = $"{Globals.currentUser.Username}-worlds",
                @private = true,
            });
            StringContent stringContent = new StringContent(content, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"{userUrl}/repos", stringContent);
            Console.WriteLine("\nRemote repository created.");
            Console.WriteLine("Doing first commit.");
        }

        public static async Task Push(HttpClient client = null) {
            if(client is null)
                client = CreateBaseClient();
            
            Byte[] bytes = File.ReadAllBytes($"{Globals.LOCALREPO_PATH}/README.md");
            string file = Convert.ToBase64String(bytes);

            string content = JsonConvert.SerializeObject(new
            {
                message = "Initial commit",
                content = file
            });
            StringContent stringContent = new StringContent(content, Encoding.UTF8, "application/vnd.github.v3.object");

            HttpResponseMessage response = await client.PostAsync($"{repoUrl}/contents/README.md", stringContent);
            
            string body = await response.Content.ReadAsStringAsync();
            Console.Write(body);
        }

        public static async Task Root(HttpClient client = null) {
            if(client is null)
                client = CreateBaseClient();

            HttpResponseMessage response = await client.GetAsync(baseUrl);
            string body = await response.Content.ReadAsStringAsync();
            JsonConvert.DeserializeObject(body);
        }
    }
}