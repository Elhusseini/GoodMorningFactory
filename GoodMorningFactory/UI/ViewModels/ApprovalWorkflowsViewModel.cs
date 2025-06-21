using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GoodMorningFactory.UI.Commands;

namespace GoodMorningFactory.UI.ViewModels
{
    public class ApprovalWorkflowsViewModel : ViewModelBase
    {
        private readonly DatabaseContext _context;

        public ObservableCollection<ApprovalWorkflow> Workflows { get; private set; }
        public ObservableCollection<Role> Roles { get; private set; }
        public Array DocumentTypes { get; private set; }

        private ApprovalWorkflow _selectedWorkflow;
        public ApprovalWorkflow SelectedWorkflow
        {
            get => _selectedWorkflow;
            set
            {
                if (_selectedWorkflow != value)
                {
                    _selectedWorkflow = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsDetailsPanelEnabled));

                    // *** بداية الإصلاح النهائي ***
                    if (_selectedWorkflow != null)
                    {
                        // التأكد من أن القائمة محملة من قاعدة البيانات
                        if (!_context.Entry(_selectedWorkflow).Collection(w => w.Steps).IsLoaded)
                        {
                            _context.ApprovalWorkflowSteps
                                    .Where(s => s.ApprovalWorkflowId == _selectedWorkflow.Id)
                                    .OrderBy(s => s.StepOrder)
                                    .Load();
                        }

                        // بدلاً من إنشاء قائمة جديدة، نقوم بتحديث القائمة الحالية
                        // هذا هو مفتاح حل المشكلة
                        var stepsList = _selectedWorkflow.Steps.OrderBy(s => s.StepOrder).ToList();
                        _selectedWorkflow.Steps.Clear();
                        foreach (var step in stepsList)
                        {
                            _selectedWorkflow.Steps.Add(step);
                        }
                    }
                    // *** نهاية الإصلاح النهائي ***
                }
            }
        }

        public bool IsDetailsPanelEnabled => SelectedWorkflow != null;

        public ICommand AddNewWorkflowCommand { get; }
        public ICommand SaveChangesCommand { get; }
        public ICommand DeleteWorkflowCommand { get; }
        public ICommand AddStepCommand { get; }
        public ICommand DeleteStepCommand { get; }

        public ApprovalWorkflowsViewModel()
        {
            _context = new DatabaseContext();
            LoadInitialData();

            AddNewWorkflowCommand = new RelayCommand(AddNewWorkflow);
            SaveChangesCommand = new RelayCommand(SaveChanges, (param) => SelectedWorkflow != null);
            DeleteWorkflowCommand = new RelayCommand(DeleteWorkflow, (param) => SelectedWorkflow != null);
            AddStepCommand = new RelayCommand(AddStep, (param) => SelectedWorkflow != null);
            DeleteStepCommand = new RelayCommand(DeleteStep);
        }

        private void LoadInitialData()
        {
            Workflows = new ObservableCollection<ApprovalWorkflow>(_context.ApprovalWorkflows.ToList());
            Roles = new ObservableCollection<Role>(_context.Roles.OrderBy(r => r.Name).ToList());
            DocumentTypes = Enum.GetValues(typeof(DocumentType));

            OnPropertyChanged(nameof(Workflows));
            OnPropertyChanged(nameof(Roles));
            OnPropertyChanged(nameof(DocumentTypes));
        }

        private void AddNewWorkflow(object parameter)
        {
            var newWorkflow = new ApprovalWorkflow
            {
                Name = "دورة موافقات جديدة",
                IsActive = true,
                DocumentType = DocumentType.PurchaseRequisition,
                Steps = new ObservableCollection<ApprovalWorkflowStep>()
            };
            _context.ApprovalWorkflows.Add(newWorkflow);
            Workflows.Add(newWorkflow);
            SelectedWorkflow = newWorkflow;
        }

        private void AddStep(object parameter)
        {
            if (SelectedWorkflow == null) return;

            var newStep = new ApprovalWorkflowStep
            {
                StepOrder = (SelectedWorkflow.Steps.Any() ? SelectedWorkflow.Steps.Max(s => s.StepOrder) : 0) + 1,
                StepName = "خطوة جديدة"
            };

            SelectedWorkflow.Steps.Add(newStep);
        }

        private void DeleteStep(object parameter)
        {
            if (parameter is ApprovalWorkflowStep step && SelectedWorkflow != null)
            {
                SelectedWorkflow.Steps.Remove(step);
                if (step.Id != 0)
                {
                    _context.ApprovalWorkflowSteps.Remove(step);
                }
            }
        }

        private void SaveChanges(object parameter)
        {
            if (SelectedWorkflow == null) return;

            if (SelectedWorkflow.Steps.Any(s => s.ApproverRoleId == 0 || string.IsNullOrWhiteSpace(s.StepName)))
            {
                MessageBox.Show("الرجاء إكمال بيانات جميع الخطوات (الاسم والدور المسؤول).", "بيانات غير مكتملة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _context.SaveChanges();
                MessageBox.Show("تم حفظ التغييرات بنجاح.", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل حفظ التغييرات: {ex.Message}\n\n{ex.InnerException?.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteWorkflow(object parameter)
        {
            if (SelectedWorkflow == null) return;

            var result = MessageBox.Show($"هل أنت متأكد من حذف دورة الموافقة '{SelectedWorkflow.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.ApprovalWorkflows.Remove(SelectedWorkflow);
                    _context.SaveChanges();
                    Workflows.Remove(SelectedWorkflow);
                    SelectedWorkflow = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل حذف الدورة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
