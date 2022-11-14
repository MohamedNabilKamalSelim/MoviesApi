using System.ComponentModel.DataAnnotations;

namespace MoviesApi.DTOs
{
    public class CreateGenreDto
    {
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
