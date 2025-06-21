using GoodMorningFactory.Core.Services;
using GoodMorningFactory.UI.ViewModels;
using System.Windows;

namespace GoodMorningFactory.UI.Views
{
    public partial class AddEditJournalVoucherView : Window
    {
        public AddEditJournalVoucherView()
        {
            InitializeComponent();
            var viewModel = new AddEditJournalVoucherViewModel(new JournalVoucherService());
            DataContext = viewModel;

            // --- بداية التعديل: استدعاء التحميل عند تحميل النافذة ---
            Loaded += async (s, e) => await viewModel.InitializeAsync();
            // --- نهاية التعديل ---
        }
    }
}