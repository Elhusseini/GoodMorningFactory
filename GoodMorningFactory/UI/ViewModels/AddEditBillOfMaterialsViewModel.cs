// UI/ViewModels/AddEditBillOfMaterialsViewModel.cs
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.Services;
using GoodMorningFactory.UI.Commands;
using GoodMorningFactory.UI.Views; // لإعادة استخدام BillOfMaterialsItemViewModel
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GoodMorningFactory.UI.ViewModels
{
    public class AddEditBillOfMaterialsViewModel : INotifyPropertyChanged
    {
        private readonly IManufacturingService _manufacturingService;
        private readonly int? _bomId;
        private readonly int? _sourceBomIdToCopy;
        private string _windowTitle;
        private Product _selectedFinishedGood;
        private string _description;
        private string _searchMaterialText;

        public string WindowTitle { get => _windowTitle; set { _windowTitle = value; OnPropertyChanged(nameof(WindowTitle)); } }
        public ObservableCollection<Product> FinishedGoods { get; set; }
        public Product SelectedFinishedGood { get => _selectedFinishedGood; set { _selectedFinishedGood = value; OnPropertyChanged(nameof(SelectedFinishedGood)); } }
        public string Description { get => _description; set { _description = value; OnPropertyChanged(nameof(Description)); } }
        public string SearchMaterialText { get => _searchMaterialText; set { _searchMaterialText = value; OnPropertyChanged(nameof(SearchMaterialText)); } }
        public ObservableCollection<BillOfMaterialsItemViewModel> BomItems { get; set; }

        public ICommand LoadCommand { get; }
        public ICommand AddMaterialCommand { get; }
        public ICommand RemoveMaterialCommand { get; }
        public ICommand SaveCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public AddEditBillOfMaterialsViewModel(int? bomId, int? sourceBomIdToCopy)
        {
            _manufacturingService = new ManufacturingService();
            _bomId = bomId;
            _sourceBomIdToCopy = sourceBomIdToCopy;

            FinishedGoods = new ObservableCollection<Product>();
            BomItems = new ObservableCollection<BillOfMaterialsItemViewModel>();

            LoadCommand = new RelayCommand(async _ => await LoadDataAsync());
            AddMaterialCommand = new RelayCommand(async _ => await AddMaterialAsync(), _ => !string.IsNullOrWhiteSpace(SearchMaterialText));
            RemoveMaterialCommand = new RelayCommand(param => RemoveMaterial(param as BillOfMaterialsItemViewModel));
            SaveCommand = new RelayCommand(async param => await SaveAsync(param as Window), _ => SelectedFinishedGood != null && BomItems.Any());

            LoadCommand.Execute(null);
        }

        private async Task LoadDataAsync()
        {
            var goods = await _manufacturingService.GetFinishedGoodsAsync();
            foreach (var good in goods) FinishedGoods.Add(good);

            if (_bomId.HasValue) // وضع التعديل
            {
                WindowTitle = "تعديل قائمة مكونات";
                var bom = await _manufacturingService.GetBomByIdAsync(_bomId.Value);
                if (bom != null)
                {
                    SelectedFinishedGood = FinishedGoods.FirstOrDefault(p => p.Id == bom.FinishedGoodId);
                    Description = bom.Description;
                    foreach (var item in bom.BillOfMaterialsItems)
                    {
                        BomItems.Add(new BillOfMaterialsItemViewModel
                        {
                            RawMaterialId = item.RawMaterialId,
                            RawMaterialName = item.RawMaterial.Name,
                            Quantity = item.Quantity,
                            ScrapPercentage = item.ScrapPercentage
                        });
                    }
                }
            }
            else if (_sourceBomIdToCopy.HasValue) // وضع النسخ
            {
                WindowTitle = "نسخ قائمة مكونات";
                var bomToCopy = await _manufacturingService.GetBomByIdAsync(_sourceBomIdToCopy.Value);
                if (bomToCopy != null)
                {
                    Description = $"نسخة من: {bomToCopy.Description}";
                    // لا نختار المنتج النهائي للسماح للمستخدم باختيار منتج جديد
                    foreach (var item in bomToCopy.BillOfMaterialsItems)
                    {
                        BomItems.Add(new BillOfMaterialsItemViewModel
                        {
                            RawMaterialId = item.RawMaterialId,
                            RawMaterialName = item.RawMaterial.Name,
                            Quantity = item.Quantity,
                            ScrapPercentage = item.ScrapPercentage
                        });
                    }
                }
            }
            else // وضع الإضافة
            {
                WindowTitle = "إنشاء قائمة مكونات جديدة";
            }
        }

        private async Task AddMaterialAsync()
        {
            var material = await _manufacturingService.FindRawMaterialAsync(SearchMaterialText);
            if (material != null)
            {
                var existingItem = BomItems.FirstOrDefault(i => i.RawMaterialId == material.Id);
                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    BomItems.Add(new BillOfMaterialsItemViewModel { RawMaterialId = material.Id, RawMaterialName = material.Name, Quantity = 1 });
                }
                SearchMaterialText = string.Empty; // مسح حقل البحث
            }
            else
            {
                MessageBox.Show("لم يتم العثور على المادة الخام.", "بحث", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RemoveMaterial(BillOfMaterialsItemViewModel item)
        {
            if (item != null)
            {
                BomItems.Remove(item);
            }
        }

        private async Task SaveAsync(Window window)
        {
            var bom = new BillOfMaterials
            {
                Id = _bomId ?? 0,
                FinishedGoodId = SelectedFinishedGood.Id,
                Description = Description,
                BillOfMaterialsItems = BomItems.Select(item => new BillOfMaterialsItem
                {
                    RawMaterialId = item.RawMaterialId,
                    Quantity = item.Quantity,
                    ScrapPercentage = item.ScrapPercentage
                }).ToList()
            };

            try
            {
                await _manufacturingService.SaveBomAsync(bom);
                window.DialogResult = true;
                window.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"فشل حفظ القائمة: {ex.Message}", "خطأ");
            }
        }

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}