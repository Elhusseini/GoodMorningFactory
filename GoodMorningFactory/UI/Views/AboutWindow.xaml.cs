// UI/Views/AboutWindow.xaml.cs
// الكود الخلفي لنافذة "عن البرنامج"
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GoodMorningFactory.UI.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            LoadInfo();
        }

        private void LoadInfo()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // تحميل معلومات الشركة من قاعدة البيانات
                    CompanyInfo companyInfo = db.CompanyInfos.FirstOrDefault();
                    if (companyInfo != null)
                    {
                        CompanyNameTextBlock.Text = companyInfo.CompanyName;
                        AddressTextBlock.Text = companyInfo.Address;
                        PhoneNumberTextBlock.Text = $"الهاتف: {companyInfo.PhoneNumber}";

                        // عرض الشعار إذا كان موجوداً
                        if (companyInfo.Logo != null && companyInfo.Logo.Length > 0)
                        {
                            BitmapImage image = new BitmapImage();
                            using (MemoryStream stream = new MemoryStream(companyInfo.Logo))
                            {
                                stream.Position = 0;
                                image.BeginInit();
                                image.StreamSource = stream;
                                image.CacheOption = BitmapCacheOption.OnLoad;
                                image.EndInit();
                            }
                            CompanyLogoImage.Source = image;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // عرض رسالة خطأ بسيطة في حالة فشل تحميل البيانات
                CompanyNameTextBlock.Text = $"خطأ في تحميل البيانات: {ex.Message}";
            }
        }
    }
}