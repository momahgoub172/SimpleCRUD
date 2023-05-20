using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleCRUD.Models;

namespace SimpleCRUD.EFCore;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<Genre> Genres { get; set; }
    public DbSet<Movie> Movies { get; set; }
}