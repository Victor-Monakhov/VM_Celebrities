using Microsoft.AspNetCore.Mvc;
using VM_Celebrities_Back.Interfaces;
using VM_Celebrities_Back.Models;

namespace VM_Celebrities_Back.Controllers
{
    [Route("api/celebrity")]
    [ApiController]
    public class CelebrityController : ControllerBase
    {
        private readonly ICelebrityRepository _repository;
        private readonly ICelebrityScraperService _scraper;

        public CelebrityController(ICelebrityRepository repository, ICelebrityScraperService scraper)
        {
            _repository = repository;
            _scraper = scraper;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetCelebrities([FromQuery] bool reset)
        {
            var celebrities = await _repository.GetAllCelebritiesAsync();

            if (reset)
            {
                celebrities = await _scraper.ScrapeCelebritiesAsync();
                await _repository.SaveCelebritiesAsync(celebrities);
            }

            return Ok(celebrities);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCelebrity(int id)
        {
            var celebrity = await _repository.GetCelebrityByIdAsync(id);
            if (celebrity == null)
            {
                return NotFound($"Celebrity with ID {id} not found.");
            }

            var celebrities = await _repository.DeleteCelebrityAsync(id);
            return Ok(celebrities);
        }
        
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCelebrity(int id, [FromBody] Celebrity? updatedCelebrity)
        {
            if (updatedCelebrity == null || updatedCelebrity.Id != id)
            {
                return BadRequest("Invalid celebrity data.");
            }

            var updatedCelebrities = await _repository.UpdateCelebrityAsync(updatedCelebrity);
            return Ok(updatedCelebrities);
        }
        
        [HttpGet("search")]
        public async Task<IActionResult> SearchCelebrities([FromQuery] string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("Search term is required.");
            }

            var matchedCelebrities = await _repository.SearchCelebritiesByNameAsync(name);
            return Ok(matchedCelebrities);
        }
    }
}