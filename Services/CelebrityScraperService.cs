using HtmlAgilityPack;
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
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(ImdbUrl);
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
                    Id = index + 1,
                    Name = "Noname",
                    Movie = string.Empty,
                    Roles = new List<string>(),
                    Gender = string.Empty,
                    BirthDate = null,
                    ImageUrl = String.Empty,
                    Info = String.Empty
                };
                var title = node.SelectSingleNode(".//h3")?.InnerText.Trim();
                if (title != null)
                {
                    var titleArr = title.Split('.');
                    if (titleArr.Length == 2) celebrity.Name = titleArr[1].Trim();
                }

                var innerImdbUrl = node.SelectSingleNode(".//a[contains(@class, 'ipc-title-link-wrapper')]")
                    ?.GetAttributeValue("href", "").Trim();
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

                var roles = node.SelectNodes(".//li[contains(@class, 'ipc-inline-list__item')]")
                    ?.Select(role => role.InnerText.Trim()).ToList();
                if (roles != null) celebrity.Roles = roles;

                celebrity.Gender = celebrity.Roles
                    .Any(role => role.ToLower().Contains("actress")) ? "female" : "male";
                
                var movie = node.SelectSingleNode(".//a[contains(@class, 'ipc-link--base')]")
                    ?.InnerText.Trim();
                if (movie != null) celebrity.Movie = movie;

                var imageUrl = node.SelectSingleNode(".//img[contains(@class, 'ipc-image')]")
                    ?.GetAttributeValue("src", "").Trim();
                if (imageUrl != null) celebrity.ImageUrl = imageUrl;

                var info = node.SelectSingleNode(".//div[contains(@class, 'ipc-html-content-inner-div')]")
                    ?.InnerText.Trim();
                if (info != null) celebrity.Info = info;

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