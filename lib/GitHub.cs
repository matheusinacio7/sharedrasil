using System;
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
        static string baseUrl = "https://github.com";
        static string baseApiUrl = "https://api.github.com";
        static string loginUrl = "https://github.com/login";

        static string userUrl = "https://api.github.com/user";

        static string repoUrl = $"{baseUrl}/{Globals.currentBranch.Url}";
        static string repoApiUrl = $"{baseApiUrl}/repos/{Globals.currentBranch.Url}";

        static string authUrl = $"{loginUrl}/oauth/access_token";
        static string codeUrl = $"{loginUrl}/device/code";

        public static async Task<string> AddSharebuddy(string user, HttpClient client = null) {
            if(client is null)
                client = CreateBaseClient();
            
            
            StringContent content = new StringContent("", Encoding.UTF8);
            content.Headers.Add("Content-Length", "0");

            string message;
            HttpResponseMessage response = await client.PutAsync($"{repoApiUrl}/collaborators/{user}", content);

            if(response.StatusCode == HttpStatusCode.Created) {
                message = $"Successfully added share buddy! He or she must accept the invitation through the email.\nThey can also visit the repository rul directly without waiting for the email, by going to the following link:\n{repoUrl}";
            } else if (response.StatusCode == HttpStatusCode.NoContent) {
                message = "That user is already a buddy! Make sure to check spam folders in the email.";
            } else {
                message = "Something has gone wrong...";
            }

            return message;
        }

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

        public static async Task<Boolean> CheckIfExists(HttpClient client = null) {
            if(client is null)
                client = CreateBaseClient();
            
            HttpResponseMessage response = await client.GetAsync(repoApiUrl);

            Console.Write(response.StatusCode);

            HttpStatusCode statusCode = response.StatusCode;

            return (statusCode == HttpStatusCode.OK);
        } 

        public static async Task CheckPermissions(HttpClient client = null) {
            if(client is null)
                client = CreateBaseClient();

            HttpResponseMessage response = await client.GetAsync($"https://api.github.com/users/{Globals.currentUser.Username}");
            Console.Write(response.Headers.ToString());
        }

        public static async Task<Boolean> CheckIfUserIsSharebuddy(string user, HttpClient client = null) {
            if(client is null)
                client = CreateBaseClient();

            HttpResponseMessage response = await client.GetAsync($"{repoApiUrl}/collaborators/{user}");
            Boolean isBuddy = response.StatusCode == HttpStatusCode.NoContent;

            return isBuddy;
        }

        public static HttpClient CreateBaseClient() {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            client.DefaultRequestHeaders.Add("Authorization", $"token {Globals.currentUser.AccessToken.Token}");
            client.DefaultRequestHeaders.Add("User-Agent", "sharedrasil");

            return client;
        }


        public static async Task CreateRepository(HttpClient client = null) {
            if(client is null)
                client = CreateBaseClient();

            if(await CheckIfExists(client)) {
                Console.WriteLine("\nThe remote repository already exists.");
            }

            string content = JsonConvert.SerializeObject(new
            {
                name = $"sharedrasil-{Globals.currentUser.Username}-sharebranch",
                @private = true,
            });
            StringContent stringContent = new StringContent(content, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync($"{userUrl}/repos", stringContent);
            Console.WriteLine("\nRemote repository created.");
            
            Sharebranch sharebranch = new Sharebranch();
            sharebranch.CreateRaw(Globals.currentUser.Username);
            sharebranch.Save();
            Console.WriteLine("\nCreated sharebranch local info and saved to file");

            if(Globals.currentBranch is null) {
                Globals.currentBranch = sharebranch;
                Console.WriteLine("\nSigned in to the sharebranch that was just created.");
            }

            Console.WriteLine("\nDoing first commit.");

            string username = Globals.currentUser.Username;
            string token = Globals.currentUser.AccessToken.Token;
            string credentials = $"{username}:{token}";

            string[] commands = {
                $"cd \"{Globals.LOCALREPO_PATH}\"",
                $"git remote add origin https://{credentials}@github.com/{username}/sharedrasil-{username}-sharebranch"
            };

            ShellWorker.RunCommands(commands);
            Push();
            Console.WriteLine("\nFinished first commit.");
        }

        public static void Push() {
            LocalRepo.Backup();

            string[] commands = {
                $"cd \"{Globals.LOCALREPO_PATH}\"",
                "git checkout --orphan temp",
                "git rm --cached . -r",
                "git add ./worlds ./README.md",
                "git commit -m \"Worlds changes\"",
                "git push -f origin temp:main",
                "git add .",
                "git commit -m \"trash\"",
                "git checkout main",
                "git branch -D temp",
                "git add .",
            };

            ShellWorker.RunCommands(commands);
        }

        public static async Task Root(HttpClient client = null) {
            if(client is null)
                client = CreateBaseClient();

            HttpResponseMessage response = await client.GetAsync(baseApiUrl);
            string body = await response.Content.ReadAsStringAsync();
            JsonConvert.DeserializeObject(body);
        }
    }
}