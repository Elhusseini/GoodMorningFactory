// GoodMorningFactory/UI/ViewModels/AddEditDepartmentViewModel.cs
// *** الكود الكامل والنهائي ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditDepartmentViewModel : BaseViewModel
    {
        private readonly IDepartmentService _departmentService;
        private readonly int? _departmentId; // لتحديد وضع الإضافة أو التعديل

        private Department _department;
        public Department Department
        {
            get => _department;
            set { _department = value; OnPropertyChanged(); }
        }

        private string _windowTitle;
        public string WindowTitle
        {
            get => _windowTitle;
            set { _windowTitle = value; OnPropertyChanged(); }
        }

        public RelayCommand SaveCommand { get; }

        public AddEditDepartmentViewModel(int? departmentId)
        {
            _departmentId = departmentId;
            _departmentService = new DepartmentService(); // استخدام الخدمة الموجودة لديك
            SaveCommand = new RelayCommand(async (p) => await SaveAsync(p as Window));
            LoadDataAsync(departmentId);
        }

        private async void LoadDataAsync(int? departmentId)
        {
            try
            {
                if (departmentId.HasValue) // وضع التعديل
                {
                    WindowTitle = "تعديل قسم";
                    Department = await _departmentService.GetDepartmentByIdAsync(departmentId.Value);
                }
                else // وضع الإضافة
                {
                    WindowTitle = "إضافة قسم جديد";
                    Department = new Department
                    {
                        // جلب الرقم التالي وعرضه (للقراءة فقط)
                        Id = await _departmentService.GetNextDepartmentIdAsync()
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveAsync(Window window)
        {
            if (string.IsNullOrWhiteSpace(Department.Name))
            {
                MessageBox.Show("يرجى إدخال اسم القسم.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_departmentId.HasValue) // تحديث القسم الحالي
                {
                    // نقوم بتحديث الكائن الذي قمنا بتحميله مباشرة
                    Department.Name = Department.Name.Trim();
                    Department.Description = Department.Description?.Trim() ?? string.Empty;
                    await _departmentService.UpdateDepartmentAsync(Department);
                }
                else // إضافة قسم جديد
                {
                    // ننشئ كائنًا جديدًا للحفظ لتجنب إرسال الـ Id المقترح
                    var newDepartment = new Department
                    {
                        Name = Department.Name.Trim(),
                        Description = Department.Description?.Trim() ?? string.Empty
                    };
                    await _departmentService.AddDepartmentAsync(newDepartment);
                }
                window.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء حفظ البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}