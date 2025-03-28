using HtmlAgilityPack;
using Microsoft.Playwright;
using VM_Celebrities_Back.Interfaces;
using VM_Celebrities_Back.Models;
namespace VM_Celebrities_Back.Services
{
    public class CelebrityScraperService : ICelebrityScraperService
    {
        private const string BaseUrl = "https://www.imdb.com";
        private const string ImdbUrl = "https://www.imdb.com/list/ls052283250/";

        public async Task<IList<Celebrity>> ScrapeCelebritiesAsync()
        {
            var celebrities = new List<Celebrity>();
            var doc = new HtmlDocument();

            try
            {
                //I'm using both Playwright and HtmlAgilityPack for scraping the page.
                //Playwright is more heavyweight and slower than HtmlAgilityPack,
                //but it allows scraping dynamic pages with lazy-loaded content.
                //HtmlAgilityPack is faster for static pages and works well in multithreaded operations.
                //To retrieve the birthdate, the scraper needs to check another (static) page.
                //This task runs in parallel since it is the most expensive operation.
                //And also you should set userAgent, because otherwise, you will get "403 forbidden" as response from imdb.
                //I added HtmlAgilityPack only approach into catch block for you, so will see at least 25 items, 
                //if you missed to run "powershell -ExecutionPolicy Bypass -File .\bin\Debug\net8.0\playwright.ps1 install" command.

                using var playwright = await Playwright.CreateAsync();
                await using var browser =
                    await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                                "Chrome/122.0.0.0 Safari/537.36",
                    Locale = "en-US"
                });

                var page = await context.NewPageAsync();
                await page.GotoAsync(ImdbUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 60000
                });

                await page.EvaluateAsync(
                    @"async () => { window.scrollTo(0, document.body.scrollHeight); await new Promise(resolve => setTimeout(resolve, 3000));}");

                var content = await page.ContentAsync();

                doc.LoadHtml(content);
            }
            catch
            {
                var web = new HtmlWeb();
                doc = await web.LoadFromWebAsync(ImdbUrl);
            }

            var celebrityNodes = doc.DocumentNode
                ?.SelectNodes("//li[contains(@class, 'ipc-metadata-list-summary-item')]");

            if (celebrityNodes == null) return celebrities;

            var tasks = new List<Task<Celebrity?>>();

            for (var i = 0; i < celebrityNodes.Count; ++i)
            {
                tasks.Add(ScrapeCelebrityAsync(celebrityNodes.ElementAt(i), i));
            }

            var results = await Task.WhenAll(tasks);

            celebrities.AddRange(results.Where(c => c != null)!);

            return celebrities;
        }

        private async Task<Celebrity?> ScrapeCelebrityAsync(HtmlNode node, int index)
        {
            try
            {
                var celebrity = new Celebrity
                {
                    Id = index + 1
                };

                var title = node.SelectSingleNode(".//h3")?.InnerText.Trim();
                if (title != null)
                {
                    var titleArr = title.Split('.');
                    if (titleArr.Length >= 2) celebrity.Name = titleArr[1].Trim();
                }

                var innerImdbUrl = node.SelectSingleNode(".//a[contains(@class, 'ipc-title-link-wrapper')]")?.GetAttributeValue("href", "").Trim();

                if (innerImdbUrl != null)
                {
                    var fullProfileUrl = $"{BaseUrl}{innerImdbUrl}";
                    var birthDate = await ScrapeBirthDateAsync(fullProfileUrl);
                    if (!string.IsNullOrEmpty(birthDate))
                    {
                        if (DateTime.TryParse(birthDate, out var parsedDate))
                        {
                            celebrity.BirthDate = parsedDate;
                        }
                        else
                        {
                            Console.WriteLine("Failed to parse birth date");
                        }
                    }
                }

                celebrity.Roles = node.SelectNodes(".//li[contains(@class, 'ipc-inline-list__item')]")?.Select(role => role.InnerText.Trim()).ToList() ?? new List<string>();
                celebrity.Gender = celebrity.Roles.Any(role => role.ToLower().Contains("actress")) ? "female" : "male";
                celebrity.Movie = node.SelectSingleNode(".//a[contains(@class, 'ipc-link--base')]")?.InnerText.Trim() ?? string.Empty;
                celebrity.ImageUrl = node.SelectSingleNode(".//img[contains(@class, 'ipc-image')]")?.GetAttributeValue("src", "").Trim() ?? string.Empty;
                celebrity.Info = node.SelectSingleNode(".//div[contains(@class, 'ipc-html-content-inner-div')]")?.InnerText.Trim() ?? string.Empty;
                
                return celebrity;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scraping celebrity: {ex.Message}");
                return null;
            }
        }

        private async Task<string> ScrapeBirthDateAsync(string url)
        {
            try
            {
                var web = new HtmlWeb();
                var profileDoc = await web.LoadFromWebAsync(url);
                var birthDate = profileDoc.DocumentNode
                    .SelectSingleNode("//div[@data-testid='birth-and-death-birthdate']/span[2]")
                    ?.InnerText.Trim();

                if (birthDate != null)
                {
                    return birthDate;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scraping birth date from {url}: {ex.Message}");
            }

            return string.Empty;
        }
    }
}