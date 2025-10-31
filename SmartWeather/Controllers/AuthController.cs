using Core.Enums;
using Managers.auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.requests.auth;
using Models.SqlEntities;

namespace SmartWeather.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AuthManager _authManager;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AuthManager authManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authManager = authManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterReq model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser()
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, GroupRole.Member.ToString());

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginReq model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized("Invalid login attempt.");

            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Invalid login attempt.");

            var token = await _authManager.GenerateJwtToken(user);

            return Ok(new { token });
        }
    }
}
