// GoodMorningFactory/UI/ViewModels/DepartmentViewModel.cs
using System;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel يمثل بيانات قسم واحد كما ستُعرض في الواجهة (مثلاً، في صف من جدول).
    /// يرث من BaseViewModel لتفعيل خاصية الإشعار بالتغييرات (INotifyPropertyChanged).
    /// </summary>
    public class DepartmentViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
