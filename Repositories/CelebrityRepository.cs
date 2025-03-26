using System.Text.Json;
using VM_Celebrities_Back.Models;

namespace VM_Celebrities_Back.Repositories
{

    public interface ICelebrityRepository
    {
        Task<List<Celebrity>> GetAllCelebritiesAsync();
        Task SaveCelebritiesAsync(List<Celebrity> celebrities);
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
    }
}