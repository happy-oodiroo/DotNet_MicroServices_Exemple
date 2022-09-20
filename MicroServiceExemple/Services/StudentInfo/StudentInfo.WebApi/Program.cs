using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using StudentInfo.Infrastructure;
using StudentInfo.Infrastructure.Persistence;

namespace StudentInfo.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigurationManager configuration = builder.Configuration;
            // For Entity Framework
            builder.Services.AddDbContext<StudentInfoDbContext>(options => options.UseSqlServer(
                configuration.GetConnectionString("ConnStr")
                ,
                x =>
                {
                    x.MigrationsAssembly("StudentInfo.Infrastructure");
                    x.UseRelationalNulls();
                }

                )
            );

            // For Identity
            builder.Services.AddIdentity<Domain.Entities.Student, IdentityRole>(options =>
            {
                // Default Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
            })
                .AddEntityFrameworkStores<StudentInfoDbContext>()
                .AddDefaultTokenProviders();

            // Adding Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                // Adding Jwt Bearer
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]))
                    };
                });

            builder.Services.AddControllers();

            //Configuring Swagger/OpenAPI 
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => {
                c.EnableAnnotations();
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Student Info API",
                    Version = "v1"
                });
                c.AddSecurityDefinition("JWT", new OpenApiSecurityScheme()
                {
                    Type = SecuritySchemeType.Http,
                    Name = "Bearer",
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme() {
                            Reference = new OpenApiReference() {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "JWT"
                            }
                        },
                        new string[0]
                    },
                });
            });


            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger(c => {
                    c.RouteTemplate = "studentinfo/swagger/{documentname}/swagger.json";
                });
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/studentinfo/swagger/v1/swagger.json", "Student Info API");
                    c.RoutePrefix = "studentinfo/swagger";
                    c.OAuthUsePkce();
                });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. To change this for production scenarios, see: https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // migrate any database changes on startup (includes initial db creation)
            using (var scope = app.Services.CreateScope())
            {
                var dataContext = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.StudentInfoDbContext>();
                    dataContext.Database.Migrate();
                var userManager = scope.ServiceProvider.GetService<UserManager<Domain.Entities.Student>>();
                StudentInfoDbInitializer.SeedUsers(userManager);
            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}