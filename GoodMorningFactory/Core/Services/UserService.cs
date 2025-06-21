using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using GoodMorningFactory.UI.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GoodMorningFactory.Core.Services
{
    public class UserService : IUserService
    {
        public async Task<PaginatedResult<UserViewModel>> GetUsersAsync(UserFilterCriteria criteria)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.Users.Include(u => u.Role).Include(u => u.Department).AsQueryable();

                if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                {
                    string searchTextLower = criteria.SearchText.ToLower();
                    query = query.Where(u => u.Username.ToLower().Contains(searchTextLower) ||
                                             (u.FirstName + " " + u.LastName).ToLower().Contains(searchTextLower) ||
                                             u.Email.ToLower().Contains(searchTextLower));
                }

                if (criteria.IsActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == criteria.IsActive.Value);
                }

                var totalItems = await query.CountAsync();
                var users = await query.OrderBy(u => u.Username)
                                       .Skip((criteria.Page - 1) * criteria.PageSize)
                                       .Take(criteria.PageSize)
                                       .ToListAsync();

                var userViewModels = users.Select(u => new UserViewModel
                {
                    Id = u.Id,
                    RoleId = u.RoleId,
                    Username = u.Username,
                    FullName = $"{u.FirstName} {u.LastName}".Trim(),
                    Email = u.Email,
                    RoleName = u.Role?.Name,
                    DepartmentName = u.Department?.Name ?? "N/A",
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    ProfilePicture = LoadImage(u.ProfilePicture)
                }).ToList();

                return new PaginatedResult<UserViewModel> { Items = userViewModels, TotalCount = totalItems };
            }
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Users.FindAsync(userId);
            }
        }

        public async Task ToggleUserStatusAsync(int userId)
        {
            using (var db = new DatabaseContext())
            {
                var user = await db.Users.FindAsync(userId);
                if (user != null)
                {
                    if (user.Username.ToLower() == "admin")
                    {
                        throw new InvalidOperationException("لا يمكن تغيير حالة حساب المسؤول الرئيسي.");
                    }
                    user.IsActive = !user.IsActive;
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task AddUserAsync(User user)
        {
            using (var db = new DatabaseContext())
            {
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            using (var db = new DatabaseContext())
            {
                db.Users.Update(user);
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Roles.ToListAsync();
            }
        }

        public async Task<List<Department>> GetDepartmentsAsync()
        {
            using (var db = new DatabaseContext())
            {
                return await db.Departments.ToListAsync();
            }
        }

        public async Task<bool> IsUsernameTakenAsync(string username, int? userId = null)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Users.AnyAsync(u => u.Id != userId && u.Username.ToLower() == username.ToLower());
            }
        }

        public async Task<bool> IsEmailTakenAsync(string email, int? userId = null)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Users.AnyAsync(u => u.Id != userId && u.Email.ToLower() == email.ToLower());
            }
        }

        private BitmapImage LoadImage(byte[] imageData)
        {
            BitmapImage image = null;
            if (imageData != null && imageData.Length > 0)
            {
                image = new BitmapImage();
                using (var mem = new MemoryStream(imageData))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
            }
            else
            {
                try
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string imagePath = Path.Combine(baseDirectory, "Assets", "default-user.png");
                    if (File.Exists(imagePath))
                    {
                        image = new BitmapImage(new Uri(imagePath));
                    }
                }
                catch { /* Handle potential file errors */ }
            }
            if (image != null) image.Freeze();
            return image;
        }
    }
}
