using System.Text.Json;
using VM_Celebrities_Back.Interfaces;
using VM_Celebrities_Back.Models;

namespace VM_Celebrities_Back.Repositories
{
    public class CelebrityRepository : ICelebrityRepository
    {
        private readonly string _filePath = "celebrities.json";
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public async Task<IList<Celebrity>> GetAllCelebritiesAsync()
        {
            await _lock.WaitAsync();
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new List<Celebrity>();
                }

                try
                {
                    var json = await File.ReadAllTextAsync(_filePath);
                    return JsonSerializer.Deserialize<List<Celebrity>>(json) ?? new List<Celebrity>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to read celebrities: {ex.Message}");
                    return new List<Celebrity>();
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SaveCelebritiesAsync(IList<Celebrity> celebrities)
        {
            var json = JsonSerializer.Serialize(celebrities, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task<Celebrity?> GetCelebrityByIdAsync(int id)
        {
            var celebrities = await GetAllCelebritiesAsync();
            return celebrities.FirstOrDefault(c => c.Id == id);
        }

        public async Task<IList<Celebrity>> DeleteCelebrityAsync(int id)
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

        public async Task<IList<Celebrity>> UpdateCelebrityAsync(Celebrity updatedCelebrity)
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

        public async Task<IList<Celebrity>> SearchCelebritiesByNameAsync(string search)
        {
            var celebrities = await GetAllCelebritiesAsync();

            if (string.IsNullOrEmpty(search))
            {
                return celebrities;
            }

            var filteredCelebrities = celebrities
                .Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return filteredCelebrities;
        }
        
        public async Task<IList<Celebrity>> AddCelebrityAsync(Celebrity newCelebrity)
        {
            var celebrities = await GetAllCelebritiesAsync();
            var celebrityToAdd = new Celebrity
            {
                Id = celebrities.Count + 1,
                Name = newCelebrity.Name,
                Gender = newCelebrity.Gender,
                BirthDate = newCelebrity.BirthDate,
                ImageUrl = newCelebrity.ImageUrl,
                Movie = newCelebrity.Movie,
                Roles = newCelebrity.Roles,
                Info = newCelebrity.Info,
            };
            
            celebrities.Add(celebrityToAdd);
            await SaveCelebritiesAsync(celebrities);
            
            return celebrities;
        }
    }
}