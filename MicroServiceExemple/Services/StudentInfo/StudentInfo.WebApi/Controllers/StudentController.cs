using StudentInfo.Domain.Entities;
using StudentInfo.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using StudentInfo.WebApi.Helpers;

namespace StudentInfo.WebApi.Contracts
{
    [Route("StudentInfo/api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly UserManager<StudentInfo.Domain.Entities.Student> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public StudentController(
            UserManager<Domain.Entities.Student> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }
        private JwtSecurityToken CreateToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);

            var token = new JwtSecurityToken(
                expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        [HttpPost]
        [Route("login")]
        [SwaggerResponse(StatusCodes.Status200OK, "login successful", typeof(LoginResponce))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request", typeof(MessageResponse))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Login failed", typeof(MessageResponse))]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                var errorList = ModelState.Values.SelectMany(m => m.Errors)
                                 .Select(e => e.ErrorMessage)
                                 .Aggregate((i, j) => i + Environment.NewLine + j);
                return BadRequest(new MessageResponse(errorList) );
            }
            var user = await _userManager.FindByNameAsync(model.UniqueIdentifier) ?? await _userManager.FindByEmailAsync(model.UniqueIdentifier);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserName.EmptyIfNull()),
                    new Claim(ClaimTypes.Name,$"{user.FirstName.EmptyIfNull()} {user.LastName.EmptyIfNull()}"  ),
                    new Claim(ClaimTypes.Email, user.Email.EmptyIfNull()),
                    new Claim(ClaimTypes.MobilePhone,user.PhoneNumber.EmptyIfNull() ),
                    new Claim(ClaimTypes.PostalCode,user.ZipCode.EmptyIfNull()),
                    new Claim(ClaimTypes.DateOfBirth,(user.Birthday?.ToString()).EmptyIfNull()),
                    new Claim(ClaimTypes.StreetAddress,user.AddressLines.EmptyIfNull()),
                    new Claim(ClaimTypes.Country,user.Country.EmptyIfNull()),
                    new Claim(ClaimTypes.StateOrProvince,user.State.EmptyIfNull()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = CreateToken(authClaims);

                await _userManager.UpdateAsync(user);

                return Ok( 
                    new LoginResponce {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo.ToUniversalTime()
                });
            }
            return Unauthorized(new MessageResponse("Login failed"));
        }

        [HttpPost]
        [Route("register")]
        [SwaggerResponse(StatusCodes.Status201Created, "Registration successful", typeof(MessageResponse))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request", typeof(MessageResponse))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal ServerError", typeof(MessageResponse))]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                var errorList = ModelState.Values.SelectMany(m => m.Errors)
                                 .Select(e => e.ErrorMessage)
                                 .Aggregate((i, j) => i + Environment.NewLine + j);
                return BadRequest(new MessageResponse(errorList));
            }

            var userExists = await _userManager.FindByNameAsync(model.UniqueIdentifier);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError,new MessageResponse( "User already exists!" ));

            Student user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UniqueIdentifier,
                FirstName = model.FirstName,
                LastName=model.LastName,
                State = model.State,
                AddressLines = model.AddressLines, 
                ZipCode = model.ZipCode,
                Country = model.Country,
                Birthday= model.Birthday,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError,new MessageResponse( "Student registration failed! Please check student details and try again."));

            return StatusCode(StatusCodes.Status201Created, new MessageResponse("Student registred successfully!"));
        }

        [Authorize]
        [HttpPut]
        [Route("ChangePassword")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Bad request", typeof(MessageResponse))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Internal ServerError", typeof(MessageResponse))]
        [SwaggerResponse(StatusCodes.Status200OK, "Success",typeof(MessageResponse))]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordModel)
        {
            if (changePasswordModel is null)
                ModelState.AddModelError("Model", "Unauthorized null model");
            if (!ModelState.IsValid)
            {
                var errorList = ModelState.Values.SelectMany(m => m.Errors)
                                 .Select(e => e.ErrorMessage)
                                 .Aggregate((i, j) => i + Environment.NewLine + j);
                return BadRequest(new MessageResponse(errorList));
            }
            string? username = (User.Identity as ClaimsIdentity)?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var currentUser = await _userManager.FindByNameAsync(username);

            IdentityResult result = await _userManager.ChangePasswordAsync(currentUser, changePasswordModel?.OldPassword, changePasswordModel?.NewPassword);

            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new MessageResponse("Password change failed! Try Again." ));
            return StatusCode(StatusCodes.Status200OK, new MessageResponse("Password changed successfully!"));
        }


    }
}
