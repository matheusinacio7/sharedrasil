using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace sharedrasil {
    public class Sharebranch {
        public static string FilePath = Path.Combine(Globals.CONFIG_PATH, "sharebranches.json");
        
        public static List<Sharebranch> GetBranchesList() {
            List<Sharebranch> branchList = new List<Sharebranch>();

            if(File.Exists(Sharebranch.FilePath)) {
                string jsonString = File.ReadAllText(Sharebranch.FilePath);
                branchList = JsonConvert.DeserializeObject<List<Sharebranch>>(jsonString);
            } 

            return branchList; 
        }

        public static async Task<Sharebranch> SignIn(string creator) {
            Sharebranch previousBranch = Globals.currentBranch;

            Sharebranch newBranch = new Sharebranch();
            newBranch.CreateRaw(creator);

            Globals.currentBranch = newBranch;

            Boolean branchExists = await Github.CheckIfExists();

            if(branchExists) {
                newBranch.Save();
                Globals.currentUser.SignedInBranch = newBranch;
                return newBranch;
            } else {
                Globals.currentBranch = previousBranch;
                return null;
            }
        }

        public string Creator {get; set;}
        public List<string> ShareBuddies {get; set;}
        public string Url {get; set;}
        public string AbsoluteUrl {get; set;}

        public void CreateRaw(string creator) {
            this.Creator = creator;
            this.ShareBuddies = new List<string>();
            this.ShareBuddies.Add(creator);
            this.Url = $"{creator}/sharedrasil-{creator}-sharebranch";
            this.AbsoluteUrl = $"https://github.com/{this.Url}";
        }


        public void Save() {
            List<Sharebranch> branchList = GetBranchesList();

            branchList.Add(this);
            string jsonString = JsonConvert.SerializeObject(branchList, Formatting.Indented);

            using(StreamWriter sw = new StreamWriter(Sharebranch.FilePath)) {
                sw.Write(jsonString);
                sw.Close();
            }
        }

    }
}