using RegisterAPI.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RegisterAPI.Db
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }

}
