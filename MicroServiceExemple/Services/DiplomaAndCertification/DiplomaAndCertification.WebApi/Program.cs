using EventBus.Messages.Common;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DiplomaAndCertification.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigurationManager configuration = builder.Configuration;
            // For Entity Framework
            builder.Services.AddDbContext<Models.Persistence.DiplomaAndCertificationDbContext>(options => options.UseSqlServer(
                configuration.GetConnectionString("ConnStr")
                ,
                x =>
                {
                    x.UseRelationalNulls();
                }

                )
            );
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
                    Title = "Diploma And Certification API",
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

            //AutoMapper
            builder.Services.AddAutoMapper(typeof(Mapper.DiplomaAndCertificationProfile));
            // MassTransit-RabbitMQ Configuration
            builder.Services.AddMassTransit(config => {

                config.UsingRabbitMq((ctx, cfg) => {
                    cfg.Host(configuration["EventBusSettings:HostAddress"]);
                    
                });
            });

            var app = builder.Build();
            
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger(c => {
                    c.RouteTemplate = "DiplomaAndCertification/swagger/{documentname}/swagger.json";
                });
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/DiplomaAndCertification/swagger/v1/swagger.json", "Diploma And Certification API");
                    c.RoutePrefix = "DiplomaAndCertification/swagger";
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
                var dataContext = scope.ServiceProvider.GetRequiredService<Models.Persistence.DiplomaAndCertificationDbContext>();
                dataContext.Database.Migrate();
            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}