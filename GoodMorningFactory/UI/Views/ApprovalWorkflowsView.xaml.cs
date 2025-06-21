// UI/Views/ApprovalWorkflowsView.xaml.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GoodMorningFactory.UI.Views
{
    public partial class ApprovalWorkflowsView : UserControl
    {
        private ApprovalWorkflow _selectedWorkflow;

        public ApprovalWorkflowsView()
        {
            InitializeComponent();
            LoadWorkflowsList();
            LoadComboBoxes();
        }

        private void LoadWorkflowsList()
        {
            using (var db = new DatabaseContext())
            {
                WorkflowsListView.ItemsSource = db.ApprovalWorkflows.ToList();
            }
        }

        private void LoadComboBoxes()
        {
            // تحميل أنواع المستندات التي تدعم الموافقات
            DocumentTypeComboBox.ItemsSource = Enum.GetValues(typeof(DocumentType))
                .Cast<DocumentType>()
                .Where(d => d == DocumentType.PurchaseRequisition) // حالياً ندعم طلبات الشراء فقط
                .Select(e => new { Value = e, Description = GetEnumDescription(e) });

            // تحميل الأدوار لعمود الموافقة
            using (var db = new DatabaseContext())
            {
                ApproverRoleColumn.ItemsSource = db.Roles.ToList();
            }
        }

        private void WorkflowsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WorkflowsListView.SelectedItem is ApprovalWorkflow workflow)
            {
                _selectedWorkflow = workflow;
                DetailsGroupBox.IsEnabled = true;

                using (var db = new DatabaseContext())
                {
                    var workflowWithDetails = db.ApprovalWorkflows
                        .Include(w => w.Steps)
                        .FirstOrDefault(w => w.Id == _selectedWorkflow.Id);

                    if (workflowWithDetails != null)
                    {
                        WorkflowNameTextBox.Text = workflowWithDetails.Name;
                        DocumentTypeComboBox.SelectedValue = workflowWithDetails.DocumentType;
                        MinimumAmountTextBox.Text = workflowWithDetails.MinimumAmount.ToString();
                        IsActiveCheckBox.IsChecked = workflowWithDetails.IsActive;
                        StepsDataGrid.ItemsSource = workflowWithDetails.Steps.OrderBy(s => s.StepOrder).ToList();
                    }
                }
            }
            else
            {
                _selectedWorkflow = null;
                DetailsGroupBox.IsEnabled = false;
                ClearDetails();
            }
        }

        private void AddWorkflowButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedWorkflow = new ApprovalWorkflow { IsActive = true };
            WorkflowsListView.SelectedItem = null;
            DetailsGroupBox.IsEnabled = true;
            ClearDetails();
            WorkflowNameTextBox.Focus();
        }

        private void SaveWorkflowButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedWorkflow == null || string.IsNullOrWhiteSpace(WorkflowNameTextBox.Text) || DocumentTypeComboBox.SelectedValue == null)
            {
                MessageBox.Show("يرجى إدخال اسم الدورة واختيار نوع المستند.", "بيانات ناقصة");
                return;
            }

            using (var db = new DatabaseContext())
            {
                if (_selectedWorkflow.Id == 0) // إضافة جديدة
                {
                    db.ApprovalWorkflows.Add(_selectedWorkflow);
                }
                else // تعديل
                {
                    db.ApprovalWorkflows.Attach(_selectedWorkflow);
                    db.Entry(_selectedWorkflow).State = EntityState.Modified;

                    // حذف الخطوات القديمة لإعادة إضافتها
                    var oldSteps = db.ApprovalWorkflowSteps.Where(s => s.ApprovalWorkflowId == _selectedWorkflow.Id);
                    db.ApprovalWorkflowSteps.RemoveRange(oldSteps);
                }

                _selectedWorkflow.Name = WorkflowNameTextBox.Text;
                _selectedWorkflow.DocumentType = (DocumentType)DocumentTypeComboBox.SelectedValue;
                decimal.TryParse(MinimumAmountTextBox.Text, out var amount);
                _selectedWorkflow.MinimumAmount = amount;
                _selectedWorkflow.IsActive = IsActiveCheckBox.IsChecked ?? false;

                // إضافة الخطوات الجديدة
                var stepsFromGrid = StepsDataGrid.ItemsSource as List<ApprovalWorkflowStep>;
                if (stepsFromGrid != null)
                {
                    foreach (var step in stepsFromGrid)
                    {
                        if (step.ApproverRoleId > 0)
                        {
                            _selectedWorkflow.Steps.Add(new ApprovalWorkflowStep
                            {
                                StepOrder = step.StepOrder,
                                StepName = step.StepName,
                                ApproverRoleId = step.ApproverRoleId
                            });
                        }
                    }
                }

                db.SaveChanges();
                MessageBox.Show("تم حفظ دورة الموافقة بنجاح.", "نجاح");
                LoadWorkflowsList();
            }
        }

        private void DeleteWorkflowButton_Click(object sender, RoutedEventArgs e)
        {
            if (WorkflowsListView.SelectedItem is ApprovalWorkflow workflowToDelete)
            {
                var result = MessageBox.Show($"هل أنت متأكد من حذف دورة الموافقة '{workflowToDelete.Name}'؟", "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var db = new DatabaseContext())
                    {
                        db.ApprovalWorkflows.Remove(workflowToDelete);
                        db.SaveChanges();
                        LoadWorkflowsList();
                    }
                }
            }
        }

        private void ClearDetails()
        {
            WorkflowNameTextBox.Clear();
            DocumentTypeComboBox.SelectedIndex = -1;
            MinimumAmountTextBox.Text = "0";
            IsActiveCheckBox.IsChecked = true;
            StepsDataGrid.ItemsSource = null;
        }

        private static string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}
