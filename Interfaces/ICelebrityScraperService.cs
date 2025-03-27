using VM_Celebrities_Back.Models;

namespace VM_Celebrities_Back.Interfaces
{
    public interface ICelebrityScraperService
    {
        Task<IList<Celebrity>> ScrapeCelebritiesAsync();
    }
}