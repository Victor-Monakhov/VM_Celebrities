using VM_Celebrities_Back.Models;

namespace VM_Celebrities_Back.Interfaces
{
    public interface ICelebrityRepository
    {
        Task<IList<Celebrity>> GetAllCelebritiesAsync();
        Task SaveCelebritiesAsync(IList<Celebrity> celebrities);
        Task<Celebrity?> GetCelebrityByIdAsync(int id);
        Task<IList<Celebrity>> DeleteCelebrityAsync(int id);
        Task<IList<Celebrity>> UpdateCelebrityAsync(Celebrity updatedCelebrity);
        Task<IList<Celebrity>> SearchCelebritiesByNameAsync(string search);
        Task<IList<Celebrity>> AddCelebrityAsync(Celebrity newCelebrity);
    }
}