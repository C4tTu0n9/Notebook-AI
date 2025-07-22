using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Synapse_API.Models.Dto.AuthDTOs;
using Synapse_API.Models.Dto.UserDTOs;
using Synapse_API.Models.Entities;
using Synapse_API.Repositories;
using Synapse_API.Services.AmazonServices;
using Synapse_API.Services.JwtServices;
using System.Security.Claims;
using Synapse_API.Utils;
using Synapse_API.Controllers;
using Synapse_API.Models.Enums;

namespace Synapse_API.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private readonly JwtService _jwtService;
        private readonly IMapper _mapper;
        private readonly EmailService _emailService;
        private readonly ILogger<UserService> _logger;
        public UserService(UserRepository repo, JwtService jwtService, IMapper mapper, EmailService emailService, ILogger<UserService> logger)
        {
            _userRepository = repo;
            _jwtService = jwtService;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger;
        }
        public async Task<User?> Authenticate(string username, string password)
        {
            // Kiểm tra xem tài khoản có tồn tại không (không kiểm tra IsActive)
            var user = await _userRepository.GetUser(username, password);

            // Nếu tài khoản không tồn tại, trả về null
            if (user == null) return null;

            // Nếu tài khoản bị vô hiệu hóa, trả về null
            if (!user.IsActive) return null;

            // Kiểm tra mật khẩu
            if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
                return null;

            return user;
        }
        public string GenerateJwtToken(User user)
        {
            return _jwtService.GenerateToken(user);
        }
        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersWithActiveAccountAsync();
            return _mapper.Map<List<UserDto>>(users);
        }
        public async Task<List<UserDto>> GetAllUserDtosAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto> GetUserDtoAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> AddUserAsync(UserRequest model)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(model.Email);
            if (existingUser != null)
            {
                throw new ArgumentException("Email already exists.");
            }
            //update infomation
            var newUser = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = (model.Role == "Admin") ? UserRole.Admin : UserRole.Student
            };

            await _userRepository.CreateUserAsync(newUser);
            return _mapper.Map<UserDto>(newUser);
        }
        public async Task<UserDto> UpdateUserAsync(UserRequest model)
        {
            var user = await _userRepository.GetUserByIdAsync(model.UserID);
            if (user == null) return null;

            var existingUserWithEmail = await _userRepository.GetUserByEmailAsync(model.Email);
            if (existingUserWithEmail != null && existingUserWithEmail.UserID != model.UserID)
            {
                throw new InvalidOperationException("Email đã tồn tại cho một tài khoản khác.");
            }

            //update infomation
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UpdatedAt = model.UpdatedAt;

            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }
            user.Role = (model.Role != null && model.Role == "Admin")
                        ? Models.Enums.UserRole.Admin
                        : Models.Enums.UserRole.Student;

            await _userRepository.UpdateUserAsync(user);
            return _mapper.Map<UserDto>(user);
        }


        public async Task<UserDto> DeleteUserAsync(int targetUserId, int currentUserId)
        {
            if (targetUserId == currentUserId)
            {
                throw new InvalidOperationException("Bạn không thể xóa chính tài khoản của mình.");
            }
            var user = await _userRepository.GetUserByIdAsync(targetUserId);
            if (user == null) return null;
            //update infomation
            user.IsActive = false;
            await _userRepository.UpdateUserAsync(user);
            return _mapper.Map<UserDto>(user);
        }

        public int GetMyUserId(ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userId);
        }
        public string GetMyEmail(ClaimsPrincipal user)
        {
            var email = user.FindFirst(ClaimTypes.Name)?.Value;
            return email;
        }

        public async Task<bool> EmailExistAsync(string email)
        {
            return await _userRepository.EmailExistAsync(email);
        }
        // Đăng ký người dùng mới
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            // kiểm tra xem email đã tồn tại hay chưa
            if (await _userRepository.EmailExistAsync(request.Email))
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Email already in use"
                };
            }

            // tạo người dùng mới
            var user = new User
            {
                Email = request.Email,
                PasswordHash = PasswordHelper.HashPassword(request.Password),
                FullName = request.FullName,
                Role = Models.Enums.UserRole.Student,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            var createdUser = await _userRepository.CreateUserAsync(user);

            var userProfile = new UserProfile
            {
                UserID = createdUser.UserID,
                Major = string.Empty,
                Goals = string.Empty,
                Interests = string.Empty,
                CVFilePath = string.Empty,
                TranscriptFilePath = string.Empty,
                DailyStudyHours = 2,
                PreferredStudyTime = "Evening",
            };

            await _userRepository.CreateUserProfileAsync(userProfile);

            // Tạo token JWT
            var token = _jwtService.GenerateToken(createdUser);

            // ---- GỌI DỊCH VỤ EMAIL ----
            try
            {
                var title = $"Chào mừng {createdUser.FullName} đến với Synapse Learn!";
                var content = "Tài khoản của bạn đã được tạo thành công. Bây giờ bạn có thể đăng nhập và bắt đầu hành trình chinh phục tri thức của mình.";
                await _emailService.SendEmailAsync(createdUser.Email, title, content);
                // Ghi log khi gửi thành công
                _logger.LogInformation("Successfully sent welcome email to {Email}", createdUser.Email);
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi chi tiết khi gửi thất bại
                _logger.LogError(ex, "Failed to send welcome email to {Email}", createdUser.Email);
            }
            return new RegisterResponse
            {
                Success = true,
                Message = "Registration successful",
                Email = createdUser.Email,
                Token = token,
                Role = Models.Enums.UserRole.Student.ToString()
            };
        }

        internal async Task<LoginResponse> LoginWithGoogleAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null || !user.IsActive)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = user == null ? "User not found." : "The account can no longer log in to the site due to a violation of community standards."
                };
            }

            // tạo token jwt
            var token = _jwtService.GenerateToken(user);
            return new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                Email = user.Email,
                Token = token,
                FullName = user.FullName,
                Role = user.Role.ToString()
            };
        }

        public async Task<ResetPasswordResponse> RequestPasswordResetAsync(string email)
        {
            // lấy dữ liệu của user
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return new ResetPasswordResponse { Success = false, Message = "This email is not registered for Synapse Learn" };
            }
            // gửi mã otp gồm 5 số về email đó
            var tokenCode = new Random().Next(10000, 999999).ToString();
            var resetToken = new Token
            {
                UserID = user.UserID,
                TokenString = tokenCode,
                ExpiryDate = DateTime.Now.AddMinutes(10),
                IsUsed = false,
            };
            // Lưu vào bảng Token
            await _userRepository.SaveTokenResetPasswordAsync(resetToken);

            // Gửi Email
            try
            {
                var title = "Mã xác thực để đặt lại mật khẩu Synapse Learn";
                var content = $"<p>Xin chào {user.FullName},</p>" +
                              $"<p>Bạn đã yêu cầu đặt lại mật khẩu. Vui lòng sử dụng mã xác thực sau để tiếp tục. Mã này sẽ hết hạn sau 10 phút.</p>" +
                              $"<h2><strong>{tokenCode}</strong></h2>" +
                              $"<p>Nếu bạn không yêu cầu điều này, vui lòng bỏ qua email này.</p>";
                await _emailService.SendEmailAsync(user.Email, title, content);
                // Ghi log khi gửi thành công
                _logger.LogInformation("Successfully sent password reset email to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                return new ResetPasswordResponse { Success = false, Message = "Failed to send reset email. Please try again later." };
            }
            return new ResetPasswordResponse { Success = true, Message = "If an account with this email exists, a password reset code has been sent." };

        }

        public async Task<ResetPasswordResponse> VerifyResetTokentAsync(string email, string token)
        {
            var resetToken = await _userRepository.GetValidTokenResetPasswordAsync(email, token);
            if (resetToken == null)
            {
                return new ResetPasswordResponse { Success = false, Message = "Invalid or expired token." };
            }
            return new ResetPasswordResponse { Success = true, Message = "Token is valid." };
        }

        public async Task<ResetPasswordResponse> ResetPasswordAsync(string email, string token, string newPassword, string confirmNewPassword)
        {
            var resetToken = await _userRepository.GetValidTokenResetPasswordAsync(email, token);
            if (resetToken == null)
            {
                return new ResetPasswordResponse { Success = false, Message = "Invalid or expired token. Please request a new one." };
            }
            var user = resetToken.User;
            user.PasswordHash = PasswordHelper.HashPassword(newPassword);
            await _userRepository.UpdateUserAsync(user);

            // Đánh dấu token đã được sử dụng
            await _userRepository.TokenIsUsedAsync(resetToken);
            return new ResetPasswordResponse { Success = true, Message = "Password has been reset successfully." };
        }


        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            var user = await _userRepository.GetUserWithProfileAsync(userId);

            return new UserProfileDto
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Email = user.Email,
                Address = user.UserProfile?.Address,
                Major = user.UserProfile?.Major,
                Interests = user.UserProfile?.Interests,
                Avatar = user.UserProfile?.Avatar,
                PhoneNumber = user.UserProfile?.PhoneNumber,
                DailyStudyHours = user.UserProfile?.DailyStudyHours,
                PreferredStudyTime = user.UserProfile?.PreferredStudyTime,
            };
        }


        public async Task<bool> UpdateUserProfileAsync(int userId, UpdateUserProfileDto dto)
        {
            return await _userRepository.UpdateUserProfileAsync(userId, dto);
        }


        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            return await _userRepository.ChangePasswordAsync(userId, dto);
        }


        public async Task<UserProfileDto> GetUserProfileByIdAsync(int userId)
        {
            var user = await _userRepository.GetUserProfileByIdAsync(userId);
            if (user == null) return null;

            return new UserProfileDto
            {
                Email = user.Email,
                FullName = user.FullName,
                Avatar = user.UserProfile?.Avatar
            };
        }


    }
}