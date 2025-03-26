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
            private const string ImdbUrl = "https://www.imdb.com/list/ls052283250/";

            public async Task<List<Celebrity>> ScrapeCelebritiesAsync()
            {
                var celebrities = new List<Celebrity>();
                var web = new HtmlWeb();
                var doc = await web.LoadFromWebAsync(ImdbUrl);
                var celebrityNodes = doc.DocumentNode
                    ?.SelectNodes("//li[contains(@class, 'ipc-metadata-list-summary-item')]");
                if (celebrityNodes == null) return celebrities;

                foreach (var node in celebrityNodes)
                {
                    var title = node.SelectSingleNode(".//h3")?.InnerText.Trim();
                    if (title == null) continue;
                    var titleArr = title.Split('.');
                    var id = Int32.Parse(titleArr[0]);
                    var name = titleArr[1].Trim();
                    var roles = node.SelectNodes(".//li[contains(@class, 'ipc-inline-list__item')]")
                        ?.Select(role => role.InnerText.Trim()).ToArray();
                    if (roles == null) continue;
                    var movie = node.SelectSingleNode(".//a[contains(@class, 'ipc-link--base')]")
                        ?.InnerText.Trim();
                    if (movie == null) continue;
                    var imageUrl = node.SelectSingleNode(".//img[contains(@class, 'ipc-image')]")
                        ?.GetAttributeValue("src", "").Trim();
                    if (imageUrl == null) continue;
                    var info = node.SelectSingleNode(".//div[contains(@class, 'ipc-html-content-inner-div')]")
                        ?.InnerText.Trim();
                    if (info == null) continue;

                    celebrities.Add(new Celebrity
                    {
                        Id = id,
                        Name = name,
                        Movie = movie,
                        Roles = roles,
                        ImageUrl = imageUrl,
                        Info = info
                    });
                }

                return celebrities;
            }
        }
    }
}