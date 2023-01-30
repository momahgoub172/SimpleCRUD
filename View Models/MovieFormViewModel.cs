using System.ComponentModel.DataAnnotations;
using SimpleCRUD.Models;

namespace SimpleCRUD.View_Models;

public class MovieFormViewModel
{
    public int Id { get; set; }
    [Required, StringLength(250)] 
    public string Title { get; set; }

    [Range(1, 10)] 
    public float Rate { get; set; }
    [Required]
    public int Year { get; set; }
    [Required, StringLength(250)] 
    public string StoryLine { get; set; }
    public byte[] Poster { get; set; }
    //RelationShips
    [Display(Name = "Genre")]
    public int GenreId { get; set;}
    public IEnumerable<Genre> Genres { get; set;}
}