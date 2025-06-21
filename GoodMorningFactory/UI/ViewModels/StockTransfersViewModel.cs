using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class StockTransfersViewModel : BaseViewModel
    {
        private readonly IStockTransferService _transferService;

        public ObservableCollection<StockTransfer> Transfers { get; set; }
        public ICommand AddTransferCommand { get; }
        public ICommand RefreshCommand { get; }

        public StockTransfersViewModel()
        {
            _transferService = new StockTransferService();
            Transfers = new ObservableCollection<StockTransfer>();
            AddTransferCommand = new RelayCommand(AddTransfer);
            RefreshCommand = new RelayCommand(async _ => await LoadTransfersAsync());
            LoadTransfersAsync();
        }

        private async Task LoadTransfersAsync()
        {
            try
            {
                var transfers = await _transferService.GetTransfersHistoryAsync();
                Transfers.Clear();
                foreach (var t in transfers)
                {
                    Transfers.Add(t);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل سجل التحويلات: {ex.Message}", "خطأ");
            }
        }

        private void AddTransfer(object parameter)
        {
            var transferWindow = new StockTransferWindow();
            if (transferWindow.ShowDialog() == true)
            {
                LoadTransfersAsync();
            }
        }
    }
}