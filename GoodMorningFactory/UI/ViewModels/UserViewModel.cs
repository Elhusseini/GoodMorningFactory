using System;
using System.Windows.Media.Imaging;

namespace GoodMorningFactory.UI.ViewModels
{
    public class UserViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public string DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public BitmapImage ProfilePicture { get; set; }
    }
}
