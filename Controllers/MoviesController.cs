using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleCRUD.EFCore;
using SimpleCRUD.Models;
using SimpleCRUD.View_Models;

namespace SimpleCRUD.Controllers;

public class MoviesController : Controller
{
    private readonly AppDbContext _context;
    private List<string> AllowedExtentions = new List<string> {".jpg", ".png"};
    private int MaxSizeOfImage = 1048576;

    public MoviesController(AppDbContext context)
    {
        _context = context;
    }
    // GET
    public async Task<IActionResult> Index()
    {
        var movies = _context.Movies.OrderByDescending(n=>n.Rate).ToList();
        return View(movies);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
         MovieFormViewModel viewModel = new MovieFormViewModel
        {
            Genres = await _context.Genres.ToListAsync()
        };
         return View(viewModel);
    }
    
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MovieFormViewModel model)
    {
        //check if model is invalid
        if (!ModelState.IsValid)
        {
            model.Genres = await _context.Genres.ToListAsync();
            return View(model);
        }
        //check that user selected file
        var Files = Request.Form.Files;
        if (!Files.Any())
        {
            model.Genres = await _context.Genres.ToListAsync();
            ModelState.AddModelError("Poster","Please select film poster");
            return View(model);
        }
        //check file ectention
        var Poster = Files.FirstOrDefault();
        if (!AllowedExtentions.Contains(Path.GetExtension(Poster.FileName).ToLower()))
        {
            model.Genres = await _context.Genres.ToListAsync();
            ModelState.AddModelError("Poster","Please select .PNG or .JPG images");
            return View(model);
        }
        //check file size
        if (Poster.Length > MaxSizeOfImage)
        {
            model.Genres = await _context.Genres.ToListAsync();
            ModelState.AddModelError("Poster","Please select poster smaller than 1MB");
            return View(model);
        }
        
        //convert poster to stream of bytes
        using var PosterStream = new MemoryStream();
        Poster.CopyToAsync(PosterStream);
        
        //map model to Movie Class
        var Movie = new Movie
        {
            Title = model.Title,
            GenreId = model.GenreId,
            Rate = model.Rate,
            Year = model.Year,
            StoryLine = model.StoryLine,
            Poster = PosterStream.ToArray()
        };
        //save movie to DB
        _context.Add(Movie);
        _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return BadRequest();
        var movie = await _context.Movies.FindAsync(id);
        if (movie == null)
            return NotFound();
        return View(movie);
    }
    
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return BadRequest();
        var movie = await _context.Movies.FindAsync(id);
        if (movie == null)
            return NotFound();
        var viewModel = new MovieFormViewModel
        {
            Id = movie.MovieId,
            Title = movie.Title,
            GenreId = movie.GenreId,
            Rate = movie.Rate,
            Year = movie.Year,
            StoryLine = movie.StoryLine,
            Poster = movie.Poster,
            Genres = await _context.Genres.OrderBy(n=>n.Name).ToListAsync()
        };
        return View(viewModel);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MovieFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Genres = await _context.Genres.ToListAsync();
            return View(model);
        }
        
        var movie = await _context.Movies.FindAsync(model.Id);
        if (movie == null)
            return NotFound();
        
        //check that user selected file
        var Files = Request.Form.Files;
        if (Files.Any())
            {
                var Poster = Files.FirstOrDefault();
                //check file ectention
                if (!AllowedExtentions.Contains(Path.GetExtension(Poster.FileName).ToLower()))
                {
                    model.Genres = await _context.Genres.ToListAsync();
                    ModelState.AddModelError("Poster","Please select .PNG or .JPG images");
                    return View(model);
                }
                //check file size
                if (Poster.Length > MaxSizeOfImage)
                {
                    model.Genres = await _context.Genres.ToListAsync();
                    ModelState.AddModelError("Poster","Please select poster smaller than 1MB");
                    return View(model);
                }

                using var PosterStream = new MemoryStream();
                Poster.CopyToAsync(PosterStream);
                movie.Poster = PosterStream.ToArray();
            }
            
        
        movie.Title = model.Title;
        movie.GenreId = model.GenreId;
        movie.Rate = model.Rate;
        movie.StoryLine = model.StoryLine;
        movie.Year = model.Year;

        _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
    

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return BadRequest();
        
        var movie = await _context.Movies.FindAsync(id);
        if (movie == null)
            return NotFound();
        
        _context.Remove(movie);
        _context.SaveChanges();
        return Ok();
    }
}