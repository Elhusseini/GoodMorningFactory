// UI/Views/AddEditLeadWindow.xaml.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditLeadWindow : Window
    {
        public AddEditLeadWindow()
        {
            InitializeComponent();
            LoadComboBoxes();
        }

        private void LoadComboBoxes()
        {
            StatusComboBox.ItemsSource = Enum.GetValues(typeof(LeadStatus))
                .Cast<LeadStatus>()
                .Select(e => new { Value = e, Description = GetEnumDescription(e) });
            StatusComboBox.SelectedValue = LeadStatus.New;

            using (var db = new DatabaseContext())
            {
                AssignedToUserComboBox.ItemsSource = db.Users.Where(u => u.IsActive).ToList();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CompanyNameTextBox.Text))
            {
                MessageBox.Show("اسم الشركة حقل مطلوب.", "بيانات ناقصة");
                return;
            }

            using (var db = new DatabaseContext())
            {
                var newLead = new Lead
                {
                    CompanyName = CompanyNameTextBox.Text,
                    ContactPerson = ContactPersonTextBox.Text,
                    Email = EmailTextBox.Text,
                    PhoneNumber = PhoneNumberTextBox.Text,
                    Status = (LeadStatus)StatusComboBox.SelectedValue,
                    AssignedToUserId = (int?)AssignedToUserComboBox.SelectedValue
                };
                db.Leads.Add(newLead);
                db.SaveChanges();
            }
            this.DialogResult = true;
        }

        private static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}
