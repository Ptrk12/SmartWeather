using Core.Enums;
using Google.Apis.Auth.OAuth2.Responses;
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
        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <remarks>
        /// Validation errors from the input model are returned as 400 Bad Request. 
        /// Identity creation errors are also returned as 400 Bad Request.
        /// </remarks>
        /// <param name="model">The required credentials for user registration.</param>
        /// <returns>Returns 200 OK on successful registration.</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        /// <summary>
        /// Authenticates the user and generates a JWT access token.
        /// </summary>
        /// <remarks>
        /// Checks the email and password against stored credentials. Returns the token object on success.
        /// </remarks>
        /// <param name="model">The user's email and password.</param>
        /// <returns>Returns 200 OK with the JWT token, or 401 Unauthorized on failed login.</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
