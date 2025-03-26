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

                
                var celebrityNodes = doc.DocumentNode.SelectNodes("//li[contains(@class, 'ipc-metadata-list-summary-item')]");
                if (celebrityNodes == null) return celebrities;

                foreach (var node in celebrityNodes)
                {
                    var title = node.SelectSingleNode(".//h3")?.InnerText.Trim() ?? "Unknown";
                    var id = Int32.Parse(title.Split('.')[0]);
                    var name = title.Substring(title.IndexOf('.') + 1).Trim();
                    // var birthDateText = node.SelectSingleNode(".//span[contains(@class, 'lister-item-year')]")?.InnerText ?? "";
                    var birthDate = new DateTime(); //DateTime.TryParse(birthDateText, out var parsedDate) ? parsedDate : DateTime.MinValue;
                    var role = string.Empty; //node.SelectSingleNode(".//p[contains(@class, 'text-muted')]/text()")?.InnerText.Trim() ?? "Unknown";
                    var imageUrl = string.Empty;// node.SelectSingleNode(".//img")?.GetAttributeValue("src", "") ?? "";

                    celebrities.Add(new Celebrity
                    {
                        Id = id,
                        Name = name,
                        BirthDate = birthDate,
                        Role = role,
                        ImageUrl = imageUrl
                    });
                }

                return celebrities;
            }
        }
    }
}