using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace P1x3lc0w.DiscordStarboardBot
{
    internal static class Saving
    {
        public static bool SaveDataExists => File.Exists("savedata.json");

        public static async Task SaveDataAsync(Data data)
        {
            if (SaveDataExists)
                File.Move("savedata.json", "savedata.old.json", true);

            await File.WriteAllTextAsync("savedata.json", JsonConvert.SerializeObject(data));
        }

        public static async Task<Data> LoadDataAsync()
        {
            return JsonConvert.DeserializeObject<Data>(await File.ReadAllTextAsync("savedata.json"));
        }
    }
}