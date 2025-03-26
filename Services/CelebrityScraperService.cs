using HtmlAgilityPack;
using VM_Celebrities_Back.Models;

namespace VM_Celebrities_Back.Services
{
    public interface ICelebrityScraperService
    {
        Task<List<Celebrity>> ScrapeCelebritiesAsync();
    }

    public class CelebrityScraperService
    {
        public class CelebrityScraper : ICelebrityScraperService
        {
            private const string BaseUrl = "https://www.imdb.com";
            private const string ImdbUrl = "https://www.imdb.com/list/ls052283250/";

            public async Task<List<Celebrity>> ScrapeCelebritiesAsync()
            {
                var celebrities = new List<Celebrity>();
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(ImdbUrl);
                var celebrityNodes = doc.DocumentNode
                    ?.SelectNodes("//li[contains(@class, 'ipc-metadata-list-summary-item')]");

                if (celebrityNodes == null) return celebrities;

                var tasks = new List<Task<Celebrity?>>();

                foreach (var node in celebrityNodes)
                {
                    tasks.Add(ScrapeCelebrityAsync(node));
                }

                var results = await Task.WhenAll(tasks);

                celebrities.AddRange(results.Where(c => c != null)!);

                return celebrities;
            }

            private async Task<Celebrity?> ScrapeCelebrityAsync(HtmlNode node)
            {
                try
                {
                    var title = node.SelectSingleNode(".//h3")?.InnerText.Trim();
                    if (title == null) return null;

                    var titleArr = title.Split('.');
                    if (titleArr.Length < 2) return null;

                    var id = int.Parse(titleArr[0]);
                    var name = titleArr[1].Trim();

                    var innerImdbUrl = node.SelectSingleNode(".//a[contains(@class, 'ipc-title-link-wrapper')]")
                        ?.GetAttributeValue("href", "").Trim();
                    if (innerImdbUrl == null) return null;

                    var fullProfileUrl = $"{BaseUrl}{innerImdbUrl}";
                    var birthDate = await ScrapeBirthDateAsync(fullProfileUrl);
                    if (birthDate == string.Empty) return null;

                    var roles = node.SelectNodes(".//li[contains(@class, 'ipc-inline-list__item')]")
                        ?.Select(role => role.InnerText.Trim()).ToArray();
                    if (roles == null) return null;

                    var gender = roles.Any(role => role.ToLower().Contains("actress")) ? "female" : "male";
                    var movie = node.SelectSingleNode(".//a[contains(@class, 'ipc-link--base')]")
                        ?.InnerText.Trim();
                    if (movie == null) return null;

                    var imageUrl = node.SelectSingleNode(".//img[contains(@class, 'ipc-image')]")
                        ?.GetAttributeValue("src", "").Trim();
                    if (imageUrl == null) return null;

                    var info = node.SelectSingleNode(".//div[contains(@class, 'ipc-html-content-inner-div')]")
                        ?.InnerText.Trim();
                    if (info == null) return null;

                    return new Celebrity
                    {
                        Id = id,
                        Name = name,
                        Movie = movie,
                        Roles = roles,
                        Gender = gender,
                        BirthDate = DateTime.Parse(birthDate),
                        ImageUrl = imageUrl,
                        Info = info
                    };
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
}