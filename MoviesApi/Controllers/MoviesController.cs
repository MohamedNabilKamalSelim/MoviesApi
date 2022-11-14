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
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private List<string> _allowedExtentions = new List<string> { ".jpg", ".png" };
        private long _maxFileSize = 1024 * 1024;

        public MoviesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMovies()
        {
            var movies = await _dbContext.Movies
                .OrderByDescending(m => m.Rate)
                .Include(x => x.Genre)
                .ToListAsync();

            return Ok(movies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var movie = await _dbContext.Movies.Include(x => x.Genre).SingleOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return NotFound();

            return Ok(movie);
        }

        [HttpGet("GetByGenreId")]
        public async Task<IActionResult> GetByGenreId(byte id)
        {
            var movies = await _dbContext.Movies
                .Where(g => g.GenreId == id)
                .OrderByDescending(m => m.Rate)
                .Include(x => x.Genre)
                .ToListAsync();

            return Ok(movies);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] MovieDto dto)
        {
            if (dto.Poster == null)
                return BadRequest("Poster is Required");

            if (dto.Poster.Length > _maxFileSize)
                return BadRequest("The max file size allowed is 1MB");

            if(!_allowedExtentions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                return BadRequest("only .png and .jpg images are allowed!");

            var isValidGenre = await _dbContext.Genres.AnyAsync(g => g.Id == dto.GenreId);
            if(!isValidGenre)
                return BadRequest($"There is no genre with ID {dto.GenreId}");

            using var dataStream = new MemoryStream();
            await dto.Poster.CopyToAsync(dataStream);

            var movie = new Movie
            {
                GenreId = dto.GenreId,
                Rate = dto.Rate,
                Poster = dataStream.ToArray(),
                StoreLine = dto.StoreLine,
                Title = dto.Title,
                Year = dto.Year
            };

            await _dbContext.Movies.AddAsync(movie);
            _dbContext.SaveChanges();

            return Ok(movie);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id,[FromForm] MovieDto dto)
        {
            var movie = await _dbContext.Movies.FindAsync(id);

            if(movie == null)
                return NotFound($"There is no movie with ID: {id}");

            var isValidGenre = await _dbContext.Genres.AnyAsync(g => g.Id == dto.GenreId);
            if (!isValidGenre)
                return BadRequest($"There is no genre with ID {dto.GenreId}");

            movie.GenreId = dto.GenreId;
            movie.Title = dto.Title;
            movie.Year = dto.Year;
            movie.StoreLine = dto.StoreLine;
            movie.Rate = dto.Rate;

            if(dto.Poster != null)
            {
                if (dto.Poster.Length > _maxFileSize)
                    return BadRequest("The max file size allowed is 1MB");

                if (!_allowedExtentions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                    return BadRequest("only .png and .jpg images are allowed!");

                using var dataStream = new MemoryStream();
                await dto.Poster.CopyToAsync(dataStream);

                movie.Poster = dataStream.ToArray();
            }

            _dbContext.SaveChanges();

            return Ok(movie);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _dbContext.Movies.FindAsync(id);

            if (movie == null)
                return NotFound($"There is no movie with ID: {id}");

            _dbContext.Movies.Remove(movie);
            _dbContext.SaveChanges();

            return Ok(movie);
        }
    }
}
