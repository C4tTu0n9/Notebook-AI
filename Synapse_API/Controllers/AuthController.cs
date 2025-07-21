using System.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Synapse_API.Models.Dto.AuthDTOs;
using Synapse_API.Models.Entities;
using Synapse_API.Repositories;
using Synapse_API.Services;
using Synapse_API.Utils;

namespace Synapse_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _configuration;

        private readonly string _frontendBaseUrl;
        private readonly string _frontendGoogleCallbackUrl;
        private readonly string _frontendLoginUrl;

        public AuthController(UserService userService, IConfiguration configuration, UserRepository userRepository)
        {
            _userService = userService;
            _configuration = configuration;

            // Đọc cấu hình một lần trong constructor
            _frontendBaseUrl = configuration.GetValue<string>("FrontendUrls:BaseUrl");
            var callbackPath = configuration.GetValue<string>("FrontendUrls:GoogleCallback");
            var loginPath = configuration.GetValue<string>("FrontendUrls:LoginError");

            _frontendGoogleCallbackUrl = $"{_frontendBaseUrl}{callbackPath}";
            _frontendLoginUrl = $"{_frontendBaseUrl}{loginPath}";
            _userRepository = userRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginRequest request)
        {
            // Tìm user mà không quan tâm đến IsActive
            var userExists = await _userRepository.GetUserByEmailWithoutActiveCheck(request.Email);

            // Nếu user tồn tại nhưng không active
            if (userExists != null && !userExists.IsActive)
            {
                return Unauthorized(new { message = "The account can no longer log in to the site due to a violation of community standards.", errorCode = "INACTIVE_ACCOUNT" });
            }

            // Tiếp tục xử lý đăng nhập bình thường
            var user = await _userService.Authenticate(request.Email, request.Password);
            if (user == null)
                return Unauthorized(new { message = "Incorrect email or password", errorCode = "INVALID_CREDENTIALS" });

            var token = _userService.GenerateJwtToken(user);

            return Ok(new LoginResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role.ToString()
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterRequest registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _userService.RegisterAsync(registerRequest);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                // Chuyển hướng về trang lỗi của Frontend
                return Redirect($"{_frontendLoginUrl}?error=google-login-failed");
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                // Chuyển hướng về trang lỗi của Frontend
                return Redirect($"{_frontendLoginUrl}?error=email-not-found");
            }

            LoginResponse loginResponse;
            var userExists = await _userService.EmailExistAsync(email);

            if (userExists)
            {
                // Người dùng đã tồn tại => đăng nhập trả về token
                loginResponse = await _userService.LoginWithGoogleAsync(email);
            }
            else
            {
                // Người dùng chưa tồn tại => đăng ký
                var randomPassword = PasswordHelper.HashPassword("123123");
                var registerRequest = new RegisterRequest
                {
                    Email = email,
                    Password = randomPassword,
                    FullName = name,
                    ConfirmPassword = randomPassword
                };
                var registerResult = await _userService.RegisterAsync(registerRequest);
                if (!registerResult.Success)
                {
                    // Chuyển hướng về trang lỗi của Frontend
                    return Redirect($"{_frontendLoginUrl}?error={Uri.EscapeDataString(registerResult.Message)}");
                }

                loginResponse = new LoginResponse
                {
                    Success = true,
                    Message = "Registration and login successful",
                    Token = registerResult.Token,
                    Email = registerResult.Email,
                    FullName = name,
                    Role = Models.Enums.UserRole.Student.ToString()
                };
            }

            // Quan trọng: Chuyển hướng về Frontend với token
            // Tạo một action mới trên Frontend tên là GoogleCallback để nhận token này
            var finalRedirectUrl = $"{_frontendGoogleCallbackUrl}?token={loginResponse.Token}&role={loginResponse.Role}";
            return Redirect(finalRedirectUrl);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromForm] ForgotPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { success = false, message = "Email is required." });
            }

            var result = await _userService.RequestPasswordResetAsync(request.Email);
            if (!result.Success)
            {
                // Trả về 500 Internal Server Error nếu có lỗi hệ thống (ví dụ: gửi email)
                return StatusCode(500, new { success = false, message = result.Message });
            }

            // Luôn trả về Ok để bảo mật, nhưng với format success và message để khớp với model frontend
            return Ok(new { success = true, message = result.Message });
        }

        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyToken([FromForm] VerifyTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.VerifyResetTokentAsync(request.Email, request.Token);
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Thêm các kiểm tra validation khác nếu cần, ví dụ: độ dài mật khẩu
            if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 6)
            {
                return BadRequest(new { success = false, message = "Password must be at least 6 characters long." });
            }

            var result = await _userService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, request.ConfirmNewPassword);
            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }
    }
}
