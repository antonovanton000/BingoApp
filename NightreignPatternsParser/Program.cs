var httpClient = new HttpClient();

var basePath = "output";
var bossName = "heolstor";
var pageLink = $"https://thefifthmatt.github.io/nightreign/{bossName}/";

var html = await httpClient.GetStringAsync(pageLink);
var doc = new HtmlAgilityPack.HtmlDocument();
doc.LoadHtml(html);


var spawngrid = doc.DocumentNode.SelectSingleNode("//div[@id='spawngrid']");
if (spawngrid != null)
{
    var fieldsets = spawngrid.Elements("fieldset");
    foreach (var fieldset in fieldsets)
    {
        var legend = fieldset.Elements("legend").FirstOrDefault();
        if (legend != null)
        {
            Console.WriteLine($"Spawn Area: {legend.InnerText}");
            var links = fieldset.Elements("a");
            if (links != null)
            {
                Console.WriteLine($"{links.Count()} images found!");
                foreach (var a in links)
                {
                    var patternPage = pageLink + a.GetAttributeValue("href", "");
                    await GetImages(httpClient, patternPage, basePath, bossName, legend.InnerText);
                }
            }
            Console.WriteLine($"----------------------------------------------------");
            Console.WriteLine($"----------------------------------------------------");
        }
    }
}


static async Task GetImages(HttpClient httpClient, string link, string basePath, string bossName, string spawnPoint)
{
    var html = await httpClient.GetStringAsync(link);
    var doc = new HtmlAgilityPack.HtmlDocument();
    doc.LoadHtml(html);
    var img = doc.DocumentNode.SelectSingleNode("//img[@class='viewerimage']");
    if (img != null)
    {
        var imgSrc = img.GetAttributeValue("src", "");
        if (!string.IsNullOrEmpty(imgSrc))
        {
            var imageUrl = new Uri(new Uri(link), imgSrc);
            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
            var destDir = System.IO.Path.Combine(basePath, bossName);
            if (!System.IO.Directory.Exists(destDir))
            {
                System.IO.Directory.CreateDirectory(destDir);
            }
            var shiftingEarthName = GetShiftingEarthName(spawnPoint);
            spawnPoint = spawnPoint.Replace($"({shiftingEarthName})", "").Trim();
            destDir = System.IO.Path.Combine(basePath, bossName, shiftingEarthName, spawnPoint);
            if (!System.IO.Directory.Exists(destDir))
            {
                System.IO.Directory.CreateDirectory(destDir);
            }

            var fileName = System.IO.Path.Combine(destDir, System.IO.Path.GetFileName(imageUrl.LocalPath));
            await System.IO.File.WriteAllBytesAsync(fileName, imageBytes);
            Console.WriteLine($"Big Image saved to {fileName}");
            
            await GetSmallImage(httpClient, imageUrl.ToString(), destDir);
            Console.WriteLine($"");
        }
    }
}

static async Task GetSmallImage(HttpClient httpClient, string imageLink, string destDir)
{
    imageLink = imageLink.Replace("/pattern", "/thumbnail");
    var imageUrl = new Uri(imageLink);
    var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
    var fileName = System.IO.Path.Combine(destDir, "_" + System.IO.Path.GetFileName(imageUrl.LocalPath));
    await System.IO.File.WriteAllBytesAsync(fileName, imageBytes);
    Console.WriteLine($"Thumbnail Image saved to {fileName}");
}

static string GetShiftingEarthName(string spawnPoint)
{
    if (spawnPoint.Contains("(Noklateo)"))
    {
        return "Noklateo";
    }
    else if (spawnPoint.Contains("(Crater)"))
    {
        return "Crater";
    }
    else if (spawnPoint.Contains("(Mountaintop)"))
    {
        return "Mountaintop";
    }
    else if (spawnPoint.Contains("(Rotted Woods)"))
    {
        return "Rotted Woods";
    }
    else
    {
        return "Default";
    }
}