using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GoodMorningFactory.UI.ViewModels
{
    /// <summary>
    /// يمثل الكلاس الأساسي الذي يجب أن ترث منه جميع كلاسات الـ ViewModel.
    /// يوفر تطبيقًا لواجهة INotifyPropertyChanged لإعلام الواجهة الرسومية (UI) بالتغييرات.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// حدث يتم إطلاقه عندما تتغير قيمة خاصية (Property).
        /// الواجهة الرسومية (UI) تستمع لهذا الحدث لتعرف متى يجب تحديث نفسها.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// دالة محمية لإطلاق الحدث PropertyChanged.
        /// [CallerMemberName] هي ميزة في C# تجعل المترجم (Compiler) يمرر اسم الخاصية التي استدعت هذه الدالة تلقائيًا.
        /// </summary>
        /// <param name="propertyName">اسم الخاصية التي تغيرت.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // التأكد من وجود مشتركين (listeners) قبل إطلاق الحدث لتجنب الأخطاء.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// دالة مساعدة آمنة لتحديث قيمة خاصية وإطلاق حدث التغيير.
        /// هذه الدالة هي الطريقة الموصى بها لتجنب أخطاء StackOverflowException.
        /// </summary>
        /// <typeparam name="T">نوع القيمة.</typeparam>
        /// <param name="backingStore">الحقل الخاص (private field) الذي يخزن القيمة الفعلية.</param>
        /// <param name="value">القيمة الجديدة المراد تعيينها.</param>
        /// <param name="propertyName">اسم الخاصية (يتم تمريره تلقائيًا).</param>
        /// <returns>تُرجع true إذا تم تغيير القيمة، و false إذا كانت القيمة الجديدة مطابقة للقديمة.</returns>
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            // الخطوة 1: المقارنة. إذا كانت القيمة لم تتغير، لا تفعل شيئًا واخرج.
            // هذا يمنع الحلقات اللانهائية ويحسن الأداء.
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            // الخطوة 2: التعيين. قم بتحديث قيمة الحقل الخاص الداعم.
            backingStore = value;

            // الخطوة 3: الإشعار. أطلق الحدث لإعلام الواجهة الرسومية بالتغيير.
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}