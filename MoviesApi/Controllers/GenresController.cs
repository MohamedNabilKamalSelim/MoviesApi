using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.Data;
using MoviesApi.DTOs;
using MoviesApi.Models;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public GenresController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGenres()
        {
            var genres = await _dbContext.Genres.OrderBy(g => g.Name).ToListAsync();

            return Ok(genres);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateGenreDto dto)
        {
            var genre = new Genre { Name = dto.Name };

            await _dbContext.Genres.AddAsync(genre);
            _dbContext.SaveChanges();

            return Ok(genre);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateGenreDto dto)
        {
            var genre = await _dbContext.Genres.SingleOrDefaultAsync(g => g.Id == id);

            if (genre == null)
                return NotFound($"There is no Genre found with id: {id}");

            genre.Name = dto.Name;
            _dbContext.SaveChanges(); 

            return Ok(genre);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var genre = await _dbContext.Genres.SingleOrDefaultAsync(g => g.Id == id);

            if (genre == null)
                return NotFound($"There is no Genre found with id: {id}");

            _dbContext.Genres.Remove(genre);
            _dbContext.SaveChanges();

            return Ok(genre);
        }
    }
}
