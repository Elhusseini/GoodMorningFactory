// UI/Views/SerialNumbersView.xaml.cs
using GoodMorningFactory.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    public partial class SerialNumbersView : UserControl
    {
        public SerialNumbersView()
        {
            InitializeComponent();
            LoadSerialNumbers();
        }

        private void LoadSerialNumbers(string filter = "")
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var query = db.SerialNumbers
                        .Include(sn => sn.Product)
                        .Include(sn => sn.StorageLocation.Warehouse)
                        .AsQueryable();

                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        query = query.Where(sn => sn.Value.Contains(filter));
                    }

                    SerialNumbersDataGrid.ItemsSource = query.OrderByDescending(sn => sn.Id).Take(200).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"فشل تحميل الأرقام التسلسلية: {ex.Message}", "خطأ");
            }
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadSerialNumbers(SearchTextBox.Text);
            }
        }
    }
}
