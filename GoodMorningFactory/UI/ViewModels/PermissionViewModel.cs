using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// ViewModel يمثل صلاحية فردية واحدة داخل مجموعة.
    /// </summary>
    public class PermissionViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        public int Id { get; set; }
        public string Description { get; set; }
        public PermissionGroupViewModel Parent { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    // إعلام المجموعة الأم عند تغيير التحديد
                    Parent?.VerifyCheckState();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// ViewModel يمثل مجموعة من الصلاحيات (وحدة)، مثل "المبيعات".
    /// </summary>
    public class PermissionGroupViewModel : INotifyPropertyChanged
    {
        private bool? _isAllSelected = false;
        public string ModuleName { get; set; }
        public ObservableCollection<PermissionViewModel> Permissions { get; set; }

        public PermissionGroupViewModel(string moduleName)
        {
            ModuleName = moduleName;
            Permissions = new ObservableCollection<PermissionViewModel>();
        }

        /// <summary>
        /// خاصية "تحديد الكل" التي ترتبط بمربع الاختيار الخاص بالمجموعة.
        /// </summary>
        public bool? IsAllSelected
        {
            get => _isAllSelected;
            set
            {
                if (_isAllSelected != value)
                {
                    _isAllSelected = value;
                    // عند تغيير حالة "تحديد الكل"، يتم تحديث جميع الصلاحيات الفرعية
                    if (_isAllSelected.HasValue)
                    {
                        SelectAll(_isAllSelected.Value);
                    }
                    OnPropertyChanged(nameof(IsAllSelected));
                }
            }
        }

        private void SelectAll(bool select)
        {
            foreach (var perm in Permissions)
            {
                perm.IsSelected = select;
            }
        }

        /// <summary>
        /// يتم استدعاؤها من الصلاحية الفرعية للتحقق من حالة "تحديد الكل".
        /// </summary>
        public void VerifyCheckState()
        {
            bool? state = null;
            if (Permissions.All(p => p.IsSelected))
            {
                state = true;
            }
            else if (Permissions.All(p => !p.IsSelected))
            {
                state = false;
            }
            _isAllSelected = state;
            OnPropertyChanged(nameof(IsAllSelected));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
