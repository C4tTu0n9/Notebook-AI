using Microsoft.EntityFrameworkCore;
using Synapse_API.Data;
using Synapse_API.Models.Entities;
using Synapse_API.Services.DatabaseServices;
using Newtonsoft.Json;
using Synapse_API.Utils;
using Synapse_API.Models.Dto.UserDTOs;

namespace Synapse_API.Repositories
{
    public class UserRepository
    {
        private readonly SynapseDbContext _context;
        private readonly RedisService _redisService;
        public UserRepository(SynapseDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }
        public async Task<User?> GetUser(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
            if (user == null) return null;
            if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
                return null;
            await _redisService.SetAsync(email, JsonConvert.SerializeObject(user), TimeSpan.FromMinutes(10));
            return user;
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<List<User>> GetAllUsersWithActiveAccountAsync()
        {
            return await _context.Users.Where(x => x.IsActive == true).ToListAsync();
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.Include(u => u.Events).FirstOrDefaultAsync(u => u.UserID == id);
            if (user == null) return null;
            return user;
        }

        public bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }

        // kiểm tra xem email đã tồn tại hay chưa
        public async Task<bool> EmailExistAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        // Tạo người dùng mới
        public async Task<User> CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        // Tạo profile mới
        public async Task<UserProfile> CreateUserProfileAsync(UserProfile userProfile)
        {
            await _context.UserProfiles.AddAsync(userProfile);
            await _context.SaveChangesAsync();
            return userProfile;
        }

        public async Task<User> GetUserAsync(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
            if (user != null && PasswordHelper.VerifyPassword(password, user.PasswordHash))
            {
                return user;
            }
            return null;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(email) && u.IsActive);
        }
        public async Task<User> GetUserByEmailWithoutActiveCheck(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        // Lưu token reset mật khẩu
        public async Task SaveTokenResetPasswordAsync(Token token)
        {
            await _context.Tokens.AddAsync(token);
            _context.SaveChanges();
        }

        // lấy token dựa trên email và mã token
        public async Task<Token?> GetValidTokenResetPasswordAsync(string email, string token)
        {
            return await _context.Tokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.User.Email == email &&
                t.TokenString == token &&
                t.ExpiryDate > DateTime.Now
                && !t.IsUsed);
        }

        // đánh dấu token đã được sử dung
        public async Task TokenIsUsedAsync(Token token)
        {
            token.IsUsed = true;
            _context.Tokens.Update(token);
            await _context.SaveChangesAsync();
        }

        //cập nhật người dùng để lưu mật khẩu mới
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetUserWithProfileAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.UserID == userId);
        }


        public async Task<bool> UpdateUserProfileAsync(int userId, UpdateUserProfileDto dto)
        {
            var user = await GetUserWithProfileAsync(userId);

            if (user == null || user.UserProfile == null)
                return false;

            // Cập nhật thông tin cá nhân
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;
            user.UserProfile.Address = dto.Address;
            user.UserProfile.PhoneNumber = dto.PhoneNumber;
            user.UserProfile.Major = dto.Major;
            user.UserProfile.Interests = dto.Interests;
            user.UserProfile.Avatar = dto.Avatar;
            user.UserProfile.DailyStudyHours = dto.DailyStudyHours;
            user.UserProfile.PreferredStudyTime = dto.PreferredStudyTime;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null) return false;


            if (!PasswordHelper.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                return false;

            // Mã hóa và cập nhật mật khẩu mới
            user.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<User?> GetUserProfileByIdAsync(int id)
        {
            var user = await _context.Users.Include(u => u.UserProfile).FirstOrDefaultAsync(u => u.UserID == id);
            if (user == null) return null;
            return user;
        }

    }
}
