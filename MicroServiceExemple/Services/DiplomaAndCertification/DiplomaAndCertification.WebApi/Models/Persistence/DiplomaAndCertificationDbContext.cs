using Microsoft.EntityFrameworkCore;

namespace DiplomaAndCertification.WebApi.Models.Persistence
{
    public class DiplomaAndCertificationDbContext : DbContext
    {
        public DiplomaAndCertificationDbContext(DbContextOptions<DiplomaAndCertificationDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<Models.Entities.Certificate> Certificates { get; set; }
    }
}
