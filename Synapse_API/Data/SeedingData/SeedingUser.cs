using Synapse_API.Models.Entities;
using Synapse_API.Models.Enums;
using System.Globalization;

namespace Synapse_API.Data.SeedingData
{
    public class SeedingUser
    {
        public static User[] GetSeedUsers()
        {
            return new User[]
            {
                new User
                {
                    UserID = 1,
                    Email = "admin@example.com",
                    PasswordHash = "$2a$11$tp/XWgvd4xVhLk5AhA3D0eJ85c5gVm70bnerCXjU0nV9Hd0XMzuvi", //123123
                    FullName = "Administrator",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.ParseExact("2025-06-07", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    UpdatedAt = DateTime.ParseExact("2025-06-07", "yyyy-MM-dd", CultureInfo.InvariantCulture)
                },
                new User
                {
                    UserID = 2,
                    Email = "student@example.com",
                    PasswordHash = "$2a$11$tp/XWgvd4xVhLk5AhA3D0eJ85c5gVm70bnerCXjU0nV9Hd0XMzuvi", //123123
                    FullName = "Student",
                    Role = UserRole.Student,
                    IsActive = true,
                    CreatedAt = DateTime.ParseExact("2025-06-07", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    UpdatedAt = DateTime.ParseExact("2025-06-07", "yyyy-MM-dd", CultureInfo.InvariantCulture)
                }
            };
        }
    }
}
