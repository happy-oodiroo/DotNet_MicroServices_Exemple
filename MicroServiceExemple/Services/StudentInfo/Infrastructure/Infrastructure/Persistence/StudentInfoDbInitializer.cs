using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentInfo.Infrastructure.Persistence
{
    public static  class StudentInfoDbInitializer
    {
        public static void SeedUsers(UserManager<Domain.Entities.Student> userManager)
        {
            if (userManager.FindByNameAsync("admin").Result == null)
            {
                Domain.Entities.Student user = new Domain.Entities.Student
                {
                    UserName = "Admin",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    Birthday= DateTime.Now
                };

                IdentityResult result = userManager.CreateAsync(user, "Admin123").Result;
            }
        }
    }
}
