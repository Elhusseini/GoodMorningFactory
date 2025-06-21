// Core/Services/CurrentUserService.cs
// خدمة مركزية لتخزين بيانات المستخدم الحالي
using GoodMorningFactory.Data.Models;

namespace GoodMorningFactory.Core.Services
{
    public static class CurrentUserService
    {
        // خاصية ثابتة (static) يمكن الوصول إليها من أي مكان في التطبيق
        public static User LoggedInUser { get; set; }
    }
}