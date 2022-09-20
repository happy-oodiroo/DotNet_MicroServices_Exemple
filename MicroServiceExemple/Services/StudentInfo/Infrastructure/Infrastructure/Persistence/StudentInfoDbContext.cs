
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace StudentInfo.Infrastructure.Persistence
{
    public class StudentInfoDbContext : IdentityDbContext<Domain.Entities.Student>
    {
        public StudentInfoDbContext(DbContextOptions<StudentInfoDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
