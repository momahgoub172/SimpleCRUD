using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleCRUD.Models;

public class Genre
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int GenreId { get; set; }
    [Required,MaxLength(250)]
    public string Name { get; set; }
}