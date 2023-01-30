using System.ComponentModel.DataAnnotations;

namespace SimpleCRUD.Models;

public class Movie
{
    public int MovieId { get; set; }  
    [Required,MaxLength(250)]
    public string Title { get; set; }
    public float Rate { get; set; }
    public int Year { get; set; }
    [Required,MaxLength(250)]
    public string StoryLine { get; set; }
    [Required]
    public byte[] Poster { get; set; }
    
    //RelationShips
    public int GenreId { get; set;}
    public IEnumerable<Genre> Genre { get; set; }
}