using System;
using Microsoft.EntityFrameworkCore;

namespace MVCBlobPetICE.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Pet> Pets { get; set; }
    }
}
