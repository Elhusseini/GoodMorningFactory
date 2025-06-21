// UI/Commands/AsyncRelayCommand.cs
// *** ملف جديد ومطلوب لإصلاح الأخطاء - الكود الكامل ***

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Commands
{
    /// <summary>
    /// كلاس لتنفيذ الأوامر غير المتزامنة (Async) في نمط MVVM.
    /// يسمح بتشغيل دوال Task مع التحكم في حالة تمكين الأمر (CanExecute) أثناء التشغيل لمنع النقرات المتعددة.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;
        private bool _isExecuting;

        // هذا الحدث يربط الأمر بنظام الأوامر في WPF ليعرف متى يجب تحديث حالة الواجهة (مثلاً، تفعيل/تعطيل زر)
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// المنشئ الخاص بالأمر غير المتزامن.
        /// </summary>
        /// <param name="execute">الدالة غير المتزامنة (async Task) التي سيتم تنفيذها.</param>
        /// <param name="canExecute">دالة اختيارية تحدد ما إذا كان يمكن تنفيذ الأمر حالياً.</param>
        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? (() => true); // إذا لم يتم توفير شرط، فالأمر متاح دائماً
        }

        /// <summary>
        /// تحدد ما إذا كان الأمر قابلاً للتنفيذ.
        /// </summary>
        public bool CanExecute(object parameter)
        {
            // لا يمكن تنفيذ الأمر إذا كان قيد التشغيل بالفعل أو إذا كان شرط التنفيذ لا يتحقق
            return !_isExecuting && _canExecute();
        }

        /// <summary>
        /// دالة التنفيذ التي تتوافق مع واجهة ICommand (لا يمكن انتظارها مباشرة).
        /// </summary>
        public async void Execute(object parameter)
        {
            await ExecuteAsync();
        }

        /// <summary>
        /// *** إضافة جديدة: دالة تُرجع Task ويمكن انتظارها (await). ***
        /// هذه الدالة هي التي سنستخدمها من الكود الخلفي لإصلاح الخطأ.
        /// </summary>
        public async Task ExecuteAsync()
        {
            if (CanExecute(null))
            {
                _isExecuting = true;
                RaiseCanExecuteChanged(); // إعلام الواجهة بتعطيل الزر المرتبط بالأمر

                try
                {
                    await _execute(); // تنفيذ الدالة الرئيسية
                }
                finally
                {
                    _isExecuting = false;
                    RaiseCanExecuteChanged(); // إعلام الواجهة بإعادة تفعيل الزر بعد انتهاء التنفيذ
                }
            }
        }
        /// <summary>
        /// دالة يدوية لتحديث حالة الأمر.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}