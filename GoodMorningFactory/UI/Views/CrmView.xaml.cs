// UI/Views/CrmView.xaml.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class CrmView : UserControl
    {
        public CrmView()
        {
            InitializeComponent();
            LoadLeads();
        }

        private void LoadLeads()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    LeadsDataGrid.ItemsSource = db.Leads
                        .Include(l => l.AssignedToUser)
                        .OrderByDescending(l => l.CreatedDate)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل العملاء المحتملين: {ex.Message}", "خطأ");
            }
        }

        private void AddLeadButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditLeadWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadLeads();
            }
        }

        private void ConvertLeadButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Lead leadToConvert)
            {
                if (leadToConvert.Status != LeadStatus.Qualified)
                {
                    MessageBox.Show("لا يمكن تحويل هذا العميل المحتمل إلا إذا كانت حالته 'مؤهل'.", "عملية غير ممكنة");
                    return;
                }

                var result = MessageBox.Show($"هل أنت متأكد من تحويل '{leadToConvert.CompanyName}' إلى عميل دائم؟", "تأكيد التحويل", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    // فتح نافذة إضافة عميل مع تعبئة البيانات من العميل المحتمل
                    var customerWindow = new AddEditCustomerWindow(new Customer
                    {
                        CustomerName = leadToConvert.CompanyName,
                        ContactPerson = leadToConvert.ContactPerson,
                        Email = leadToConvert.Email,
                        PhoneNumber = leadToConvert.PhoneNumber
                    });

                    if (customerWindow.ShowDialog() == true)
                    {
                        // يمكنك هنا حذف العميل المحتمل أو تغيير حالته إلى "تم تحويله"
                        using (var db = new DatabaseContext())
                        {
                            var lead = db.Leads.Find(leadToConvert.Id);
                            if (lead != null)
                            {
                                db.Leads.Remove(lead);
                                db.SaveChanges();
                            }
                        }
                        LoadLeads();
                    }
                }
            }
        }
    }
}
