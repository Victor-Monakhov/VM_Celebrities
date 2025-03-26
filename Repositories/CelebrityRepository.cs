using System.Text.Json;
using VM_Celebrities_Back.Models;

namespace VM_Celebrities_Back.Repositories
{

    public interface ICelebrityRepository
    {
        Task<List<Celebrity>> GetAllCelebritiesAsync();
        Task SaveCelebritiesAsync(List<Celebrity> celebrities);
        Task<Celebrity?> GetCelebrityByIdAsync(int id);
        Task<List<Celebrity>> DeleteCelebrityAsync(int id);
        Task<List<Celebrity>> UpdateCelebrityAsync(Celebrity updatedCelebrity);
        Task<List<Celebrity>> SearchCelebritiesByNameAsync(string searchTerm);
    }
    
    public class CelebrityRepository: ICelebrityRepository
    {
        private readonly string _filePath = "celebrities.json";

        public async Task<List<Celebrity>> GetAllCelebritiesAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<Celebrity>();
            }

            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<Celebrity>>(json) ?? new List<Celebrity>();
        }

        public async Task SaveCelebritiesAsync(List<Celebrity> celebrities)
        {
            var json = JsonSerializer.Serialize(celebrities, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }
        
        public async Task<Celebrity?> GetCelebrityByIdAsync(int id)
        {
            var celebrities = await GetAllCelebritiesAsync();
            return celebrities.FirstOrDefault(c => c.Id == id);
        }
        
        public async Task<List<Celebrity>> DeleteCelebrityAsync(int id)
        {
            var celebrities = await GetAllCelebritiesAsync();

            var celebrityToRemove = celebrities.FirstOrDefault(c => c.Id == id);
            if (celebrityToRemove != null)
            {
                celebrities.Remove(celebrityToRemove);
                await SaveCelebritiesAsync(celebrities);
            }

            return celebrities;
        }
        
        public async Task<List<Celebrity>> UpdateCelebrityAsync(Celebrity updatedCelebrity)
        {
            var celebrities = await GetAllCelebritiesAsync();

            var celebrityToUpdate = celebrities.FirstOrDefault(c => c.Id == updatedCelebrity.Id);
            if (celebrityToUpdate != null)
            {
                celebrityToUpdate.Name = updatedCelebrity.Name;
                celebrityToUpdate.Movie = updatedCelebrity.Movie;
                celebrityToUpdate.Roles = updatedCelebrity.Roles;
                celebrityToUpdate.Gender = updatedCelebrity.Gender;
                celebrityToUpdate.BirthDate = updatedCelebrity.BirthDate;
                celebrityToUpdate.ImageUrl = updatedCelebrity.ImageUrl;
                celebrityToUpdate.Info = updatedCelebrity.Info;

                await SaveCelebritiesAsync(celebrities);
            }

            return celebrities;
        }
        
        public async Task<List<Celebrity>> SearchCelebritiesByNameAsync(string searchTerm)
        {
            var celebrities = await GetAllCelebritiesAsync();

            if (string.IsNullOrEmpty(searchTerm))
            {
                return celebrities;
            }

            var filteredCelebrities = celebrities
                .Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return filteredCelebrities;
        }
    }
}