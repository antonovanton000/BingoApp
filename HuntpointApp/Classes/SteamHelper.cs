using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.Classes
{
    public class SteamHelper
    {
        public static async Task<string> GetUserNameById(string userId)
        {
            var httpClient = new System.Net.Http.HttpClient();
            var url = $"https://steamcommunity.com/profiles/{userId}";
            var html = await httpClient.GetStringAsync(url);
            var startIndex = html.IndexOf("<title>") + "<title>".Length;
            var endIndex = html.IndexOf("</title>", startIndex);
            if (startIndex < 0 || endIndex < 0 || startIndex >= endIndex)
            {
                return "Unknown User";
            }
            else
            {
                var userName = html.Substring(startIndex, endIndex - startIndex);
                var arr = userName.Split("::");
                if (arr.Length > 1)
                {
                    userName = arr[1].Trim();
                }
                else
                {
                    userName = "Unknown User";
                }
                return userName.Trim();
            }
        }
    }
}
