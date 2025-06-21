// UI/ViewModels/PermissionViewModel.cs
// *** ملف جديد: ViewModel لعرض الصلاحيات بشكل شجري ***
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GoodMorningFactory.UI.ViewModels
{
    public class PermissionViewModel : INotifyPropertyChanged
    {
        public int PermissionId { get; set; }
        public string Name { get; set; }
        public ObservableCollection<PermissionViewModel> Children { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    foreach (var child in Children)
                    {
                        child.IsSelected = value;
                    }
                }
            }
        }

        public PermissionViewModel(string name, bool isSelected)
        {
            Name = name;
            _isSelected = isSelected;
            Children = new ObservableCollection<PermissionViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}