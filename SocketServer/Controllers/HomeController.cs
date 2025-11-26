using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Newtonsoft.Json;
using SocketServer.Models;
using System.IO;
namespace SocketServer.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        try
        {
            var files = Directory.GetFiles("wwwroot/uploads/apps", "BingoApp*.zip");
            var file = files.FirstOrDefault();
            if (file != null)
            {
                ViewBag.Version = GetVersion(file);
                ViewBag.DownloadLink = GetDownloadLink(file);
            }
        }
        catch (Exception ex)
        {
            return Content(ex.Message);
        }
        return View();
    }

    public IActionResult Rules()
    {
        return View();
    }

    public IActionResult SekiroRules()
    {
        return View();
    }

    public IActionResult HuntpointRules()
    {
        return View();
    }

    public IActionResult BingoMod()
    {
        var files = Directory.GetFiles("wwwroot/uploads/apps", "BingoMod*.zip");
        var file = files.FirstOrDefault();
        if (file != null)
        {
            ViewBag.Version = GetVersion(file);
            ViewBag.DownloadLink = GetDownloadLink(file);
        }
        return View();
    }

    public IActionResult Downloads()
    {
        try
        {
            var files = Directory.GetFiles("wwwroot/uploads/apps", "*.zip");
            foreach (var item in files)
            {
                if (item.Contains("BingoApp"))
                {
                    ViewBag.BingoAppLink = GetDownloadLink(item);
                    ViewBag.BingoAppVersion = GetVersion(item);
                }
                if (item.Contains("BingoMod"))
                {
                    ViewBag.BingoModLink = GetDownloadLink(item);
                    ViewBag.BingoModVersion = GetVersion(item);
                }
                if (item.Contains("HuntpointMod"))
                {
                    ViewBag.HuntpointModLink = GetDownloadLink(item);
                    ViewBag.HuntpointModVersion = GetVersion(item);
                }
                if (item.Contains("GameItemTracker"))
                {
                    ViewBag.GITLink = GetDownloadLink(item);
                    ViewBag.GITVersion = GetVersion(item);
                }
                if (item.Contains("ResurectionBingo"))
                {
                    ViewBag.RBLink = GetDownloadLink(item);
                    ViewBag.RBVersion = GetVersion(item);
                }
                if (item.Contains("HuntpointApp"))
                {
                    ViewBag.HPLink = GetDownloadLink(item);
                    ViewBag.HPVersion = GetVersion(item);
                }
                if (item.Contains("FogGateHelper"))
                {
                    ViewBag.FGLink = GetDownloadLink(item);
                    ViewBag.FGVersion = GetVersion(item);
                }

            }

            var file = files.FirstOrDefault();
            if (file != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var arr = fileName.Split("_");
                ViewBag.Version = arr[1];
            }
        }
        catch (Exception ex)
        {
            return Content(ex.Message);
        }

        return View();
    }

    public async Task<IActionResult> News()
    {
        try
        {
            var newsFiles = Directory.GetFiles("wwwroot/uploads/news", "*.json");

            var news = new List<NewsItem>();
            foreach (var item in newsFiles)
            {
                try
                {
                    var json = await System.IO.File.ReadAllTextAsync(item, System.Text.Encoding.UTF8);
                    var newsItem = JsonConvert.DeserializeObject<NewsItem>(json);
                    if (newsItem != null)
                    {
                        newsItem.FileName = Path.GetFileNameWithoutExtension(item);
                        news.Add(newsItem);
                    }

                }
                catch (Exception)
                {
                }

            }
            return View(news);
        }
        catch (Exception ex)
        {
            return Content(ex.Message);
        }
    }

    public async Task<IActionResult> NewsDetail(string fileName)
    {
        try
        {
            var json = await System.IO.File.ReadAllTextAsync(Path.Combine("wwwroot/uploads/news", $"{fileName}.json"));
            var newsItem = JsonConvert.DeserializeObject<NewsItem>(json);
            newsItem.FileName = fileName;

            return View(newsItem);
        }
        catch (Exception)
        {
        }
        return NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> ContactUs()
    {
        var model = new ContactInfo();

        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> ContactUsForm(ContactInfo contactInfo)
    {
        var mailBody = $"<h1>Сообщение с сайта bingoapp.injusteam.kz</h1>" +
               $"<br/><label>Никнейм: {contactInfo.Name}</label>" +
               $"<br/><label>Discord: {contactInfo.Discord}</label>" +
               $"<br/><label>Vk: {contactInfo.Vk}</label>" +
               $"<br/><label>Другой способ: {contactInfo.Other}</label>" +
               $"<br/><label>Сообщение: {contactInfo.Message}</label>";

        await SendEmail(mailBody);

        return View("EmailSent", new EmailSentModel() { Header = "Сообщение отправлено!", Message = "Ваше сообщение успешно отправлено! <br>Скоро мы обязатльено свяжемся с Вами!" });
    }


    [HttpGet]
    public async Task<IActionResult> FindBingoPlayer()
    {
        var model = new BingoPlayer();
        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> FindBingoPlayerForm(BingoPlayer bingoPlayer)
    {
        var mailBody = $"<h1>Заявка на бинго с сайта bingoapp.injusteam.kz</h1>" +
               $"<br/><label>Никнейм: {bingoPlayer.Name}</label>" +
               $"<br/><label>Игра: {bingoPlayer.Game}</label>" +
               $"<br/><label>Уровень игрока: {bingoPlayer.GameLevel}</label>" +
               $"<br/><label>Discord: {bingoPlayer.Discord}</label>" +
               $"<br/><label>Vk: {bingoPlayer.Vk}</label>" +
               $"<br/><label>Другой способ: {bingoPlayer.Other}</label>";

        await SendEmail(mailBody);

        return View("EmailSent", new EmailSentModel() { Header = "Заявка отправлена!", Message = "Ваша заявка успешно отправлена! <br>Скоро мы обязатльено свяжемся с Вами!" });
    }


    public async Task<IActionResult> GamesResults()
    {
        var gamesResults = new List<GameResult>();
        var hpgamesResults = new List<HPGameResult>();
        var leaderBoardList = new List<LeaderboardItem>();
        var hpleaderBoardList = new List<LeaderboardItem>();

        if (System.IO.File.Exists("wwwroot/uploads/results/gamesResults.json"))
        {
            var jsonGamesResult = await System.IO.File.ReadAllTextAsync("wwwroot/uploads/results/gamesResults.json");
            if (!string.IsNullOrEmpty(jsonGamesResult))
            {
                try
                {
                    var gamesResultList = JsonConvert.DeserializeObject<List<GameResult>>(jsonGamesResult);
                    if (gamesResultList != null)
                        gamesResults = gamesResultList;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
        }

        if (System.IO.File.Exists("wwwroot/uploads/results/hpgamesResults.json"))
        {
            var jsonGamesResult = await System.IO.File.ReadAllTextAsync("wwwroot/uploads/results/hpgamesResults.json");
            if (!string.IsNullOrEmpty(jsonGamesResult))
            {
                try
                {
                    var hpgamesResultList = JsonConvert.DeserializeObject<List<HPGameResult>>(jsonGamesResult);
                    if (hpgamesResultList != null)
                        hpgamesResults = hpgamesResultList;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            }
        }


        foreach (var game in gamesResults)
        {
            for (int i = 0; i < game.PlayersNames.Length; i++)
            {
                var item = leaderBoardList.FirstOrDefault(x => x.PlayerName == game.PlayersNames[i] && x.GameName == game.GameName);

                if (item == null)
                {
                    LeaderboardItem litem = new() { PlayerName = game.PlayersNames[i], GamesCount = 1, PresetName = game.PresetName, GameName = game.GameName };
                    if (IsPlayerWinner(i, game))
                        litem.WinsCount = 1;
                    
                    leaderBoardList.Add(litem);
                }
                else
                {
                    item.GamesCount++;
                    if (IsPlayerWinner(i, game))
                        item.WinsCount ++;
                }
            }
        }

        foreach (var game in hpgamesResults)
        {
            for (int i = 0; i < game.PlayersNames.Length; i++)
            {
                var item = hpleaderBoardList.FirstOrDefault(x => x.PlayerName == game.PlayersNames[i] && x.GameName == game.GameName);

                if (item == null)
                {
                    LeaderboardItem litem = new() { PlayerName = game.PlayersNames[i], GamesCount = 1, PresetName = game.PresetName, GameName = game.GameName };
                    if (IsHPPlayerWinner(i, game))
                        litem.WinsCount = 1;

                    hpleaderBoardList.Add(litem);
                }
                else
                {
                    item.GamesCount++;
                    if (IsHPPlayerWinner(i, game))
                        item.WinsCount++;
                }
            }
        }


        ViewBag.GamesResults = gamesResults.OrderByDescending(i => i.GameDate).ToList();
        ViewBag.LeaderBoards = leaderBoardList.OrderByDescending(i => i.WinsCount).ToList();

        ViewBag.HPGamesResults = hpgamesResults.OrderByDescending(i => i.GameDate).ToList();
        ViewBag.HPLeaderBoards = hpleaderBoardList.OrderByDescending(i => i.WinsCount).ToList();

        return View();
    }

    private bool IsPlayerWinner(int playerIndex, GameResult game)
    {
        if (game.LinesCount.Length != game.LinesCount.Distinct().Count())
        {
            if (game.Score.Length != game.Score.Distinct().Count())
            {
                return false;
            }
        }

        var maxLinesCount = game.LinesCount.Max();
        var maxSquareCount = game.Score.Max();
        if (game.LinesCount[playerIndex] == maxLinesCount)
        {
            if (game.LinesCount.Count(i => i == maxLinesCount) == 1)
                return true;
            else
            {
                if (game.Score[playerIndex] == maxSquareCount) 
                    return true;
                else
                    return false; 
            }
        }
        else
        {
            return false;
        }
    }

    private bool IsHPPlayerWinner(int playerIndex, HPGameResult game)
    {
        var playerScore = game.Score.ElementAtOrDefault(playerIndex);
        var maxScore = game.Score.Max();
        return playerScore == maxScore;
    }


    public async Task<IActionResult> EmailSent()
    {
        return View();
    }

    private async Task SendEmail(string text)
    {
        //var emailFrom = "info@injusteam.kz";
        //var emailTo = "antonov-anton@outlook.com";
        //var server = "185.4.180.106";
        //var port = 25;
        //var login = "ubuntu";
        //var password = "Q12FMYbUyhKr2pSQm8ksfcM=";

        var emailFrom = "info@nrocevents.kz";
        var emailTo = "antonov-anton@outlook.com";
        var server = "smtp.yandex.ru";
        var port = 465;
        var login = emailFrom;
        var password = "Qwerty_123!";


        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("BingoApp", emailFrom));
        message.To.Add(new MailboxAddress("", emailTo));
        message.Subject = "BingoApp сообщение с сайта";

        var builder = new BodyBuilder();

        builder.HtmlBody = text;

        // Now we just need to set the message body and we're done
        message.Body = builder.ToMessageBody();
        try
        {
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(server, port, true);
                await client.AuthenticateAsync(login, password);
                await client.SendAsync(message);

                await client.DisconnectAsync(true);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

    }

    string GetVersion(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var arr = fileName.Split("_");
        return arr[1];
    }

    string GetDownloadLink(string filePath) => filePath.Replace("wwwroot", "");

    public ActionResult Huntpoint()
    {
        return View();
    }

    public ActionResult BingoAppChangeLog()
    {
        return View();
    }
}

public class EmailSentModel
{
    public string Header { get; set; }

    public string Message { get; set; }
}


public class NewsItem
{
    public string Title { get; set; }

    public DateTime Date { get; set; }

    public string Content { get; set; }

    public string FileName { get; set; }
}

public class ContactInfo
{
    public string Name { get; set; }

    public string Discord { get; set; }

    public string Vk { get; set; }

    public string Other { get; set; }

    public string Message { get; set; }
}

public class BingoPlayer
{
    public string Name { get; set; }

    public string Discord { get; set; }

    public string Vk { get; set; }

    public string Other { get; set; }

    public string Game { get; set; }
    public string GameLevel { get; set; }
    public string Platform { get; set; }
}