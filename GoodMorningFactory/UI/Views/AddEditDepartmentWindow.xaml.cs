using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditDepartmentWindow : Window
    {
        private int? _departmentId;

        public AddEditDepartmentWindow(int? departmentId = null)
        {
            InitializeComponent();
            _departmentId = departmentId;

            if (_departmentId.HasValue)
            {
                WindowTitle.Text = "تعديل قسم";
                LoadDepartment();
            }
            else
            {
                WindowTitle.Text = "إضافة قسم جديد";
                // توليد كود القسم التالي (رقم تسلسلي مقترح)
                using (var db = new DatabaseContext())
                {
                    int nextId = 1;
                    if (db.Departments.Any())
                        nextId = db.Departments.Max(d => d.Id) + 1;
                    IdTextBox.Text = nextId.ToString();
                }
            }
        }

        private void LoadDepartment()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var department = db.Departments.Find(_departmentId.Value);
                    if (department != null)
                    {
                        IdTextBox.Text = department.Id.ToString();
                        NameTextBox.Text = department.Name;
                        DescriptionTextBox.Text = department.Description;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل بيانات القسم: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("يرجى إدخال اسم القسم.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            try
            {
                using (var db = new DatabaseContext())
                {
                    Department department;

                    if (_departmentId.HasValue)
                    {
                        department = db.Departments.Find(_departmentId.Value);
                        if (department == null)
                        {
                            MessageBox.Show("لم يتم العثور على القسم المحدد", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        department = new Department();
                        db.Departments.Add(department);
                    }

                    department.Name = NameTextBox.Text.Trim();
                    department.Description = DescriptionTextBox.Text.Trim();
                    db.SaveChanges();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء حفظ البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}