using BingoApp.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Classes
{
    public class LocalNewsHelper
    {
        public static async Task SaveLocalViewedNews(NewsItem newsItem)
        {
            if (!System.IO.File.Exists("local_news.json"))
            {
                var jarr = new JArray();
                jarr.Add(newsItem.FileName);

                await System.IO.File.WriteAllTextAsync("local_news.json", jarr.ToString(Formatting.None));
            }
            else
            {

                var json = await System.IO.File.ReadAllTextAsync("local_news.json");
                var jarr = JArray.Parse(json);
                if (!jarr.Contains(newsItem.FileName))
                {
                    jarr.Add(newsItem.FileName);
                }
                await System.IO.File.WriteAllTextAsync("local_news.json", jarr.ToString(Formatting.None));
            }
        }

        public static async Task<string[]> GetLocalViewedNews()
        {
            if (!System.IO.File.Exists("local_news.json"))
            {
                return Array.Empty<string>();
            }
            while (!CanRead("local_news.json"))
            {
                await Task.Delay(500);
            }

            var json = await System.IO.File.ReadAllTextAsync("local_news.json");
            var newsIds = JsonConvert.DeserializeObject<List<string>>(json);
            return newsIds?.ToArray() ?? Array.Empty<string>();

        }

        private static bool CanRead(string fileName)
        {
            try
            {
                File.Open(fileName, FileMode.Open, FileAccess.Read).Dispose();
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
