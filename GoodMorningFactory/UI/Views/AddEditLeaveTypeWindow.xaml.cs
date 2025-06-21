// UI/Views/AddEditLeaveTypeWindow.xaml.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditLeaveTypeWindow : Window
    {
        private int? _leaveTypeId;

        public AddEditLeaveTypeWindow(int? leaveTypeId = null)
        {
            InitializeComponent();
            _leaveTypeId = leaveTypeId;
            if (_leaveTypeId.HasValue) { LoadData(); }
        }

        private void LoadData()
        {
            using (var db = new DatabaseContext())
            {
                var leaveType = db.LeaveTypes.Find(_leaveTypeId.Value);
                if (leaveType != null)
                {
                    NameTextBox.Text = leaveType.Name;
                    DescriptionTextBox.Text = leaveType.Description;
                    DaysPerYearTextBox.Text = leaveType.DaysPerYear.ToString();
                    IsPaidCheckBox.IsChecked = leaveType.IsPaid;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text)) { MessageBox.Show("اسم النوع حقل مطلوب."); return; }

            using (var db = new DatabaseContext())
            {
                LeaveType leaveType;
                if (_leaveTypeId.HasValue) { leaveType = db.LeaveTypes.Find(_leaveTypeId.Value); }
                else { leaveType = new LeaveType(); db.LeaveTypes.Add(leaveType); }

                leaveType.Name = NameTextBox.Text;
                leaveType.Description = DescriptionTextBox.Text;
                int.TryParse(DaysPerYearTextBox.Text, out int days);
                leaveType.DaysPerYear = days;
                leaveType.IsPaid = IsPaidCheckBox.IsChecked ?? false;

                db.SaveChanges();
            }
            this.DialogResult = true;
        }
    }
}
