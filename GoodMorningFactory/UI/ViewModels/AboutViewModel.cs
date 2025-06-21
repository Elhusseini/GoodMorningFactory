// GoodMorningFactory/UI/ViewModels/AboutViewModel.cs
// *** ملف جديد: ViewModel لنافذة "عن البرنامج" ***
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        private readonly ISystemInfoService _infoService;

        private string _companyName = "اسم الشركة";
        public string CompanyName
        {
            get => _companyName;
            set { _companyName = value; OnPropertyChanged(); }
        }

        private string _address;
        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(); }
        }

        private BitmapImage _companyLogo;
        public BitmapImage CompanyLogo
        {
            get => _companyLogo;
            set { _companyLogo = value; OnPropertyChanged(); }
        }

        public AboutViewModel()
        {
            _infoService = new SystemInfoService();
            LoadCompanyInfoAsync();
        }

        private async void LoadCompanyInfoAsync()
        {
            try
            {
                CompanyInfo companyInfo = await _infoService.GetCompanyInfoAsync();
                if (companyInfo != null)
                {
                    CompanyName = companyInfo.CompanyName;
                    Address = companyInfo.Address;
                    PhoneNumber = $"الهاتف: {companyInfo.PhoneNumber}";

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
                            image.Freeze(); // لتحسين الأداء
                        }
                        CompanyLogo = image;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل بيانات الشركة: {ex.Message}", "خطأ");
            }
        }
    }
}