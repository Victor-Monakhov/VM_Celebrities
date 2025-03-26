
using Microsoft.AspNetCore.Mvc;
using VM_Celebrities_Back.Repositories;
using VM_Celebrities_Back.Services;

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
        public async Task<IActionResult> GetCelebrities()
        {
            var celebrities = await _repository.GetAllCelebritiesAsync();

            if (!celebrities.Any())
            {
                celebrities = await _scraper.ScrapeCelebritiesAsync();
                await _repository.SaveCelebritiesAsync(celebrities);
            }

            return Ok(celebrities);
        }
    }
}