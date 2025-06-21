// UI/Views/MyApprovalsView.xaml.cs
using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GoodMorningFactory.UI.Views
{
    public partial class MyApprovalsView : UserControl
    {
        public MyApprovalsView()
        {
            InitializeComponent();
            LoadPendingApprovals();
        }

        private void LoadPendingApprovals()
        {
            if (CurrentUserService.LoggedInUser == null) return;

            try
            {
                using (var db = new DatabaseContext())
                {
                    var userRoleId = CurrentUserService.LoggedInUser.RoleId;
                    var pendingRequests = db.ApprovalRequests
                        .Include(ar => ar.CurrentStep)
                        .Where(ar => ar.Status == ApprovalStatus.Pending && ar.CurrentStep.ApproverRoleId == userRoleId)
                        .ToList();

                    var viewModels = pendingRequests.Select(ar =>
                    {
                        string docNumber = "";
                        string requester = "";
                        if (ar.DocumentType == DocumentType.PurchaseRequisition)
                        {
                            var pr = db.PurchaseRequisitions.Find(ar.DocumentId);
                            if (pr != null)
                            {
                                docNumber = pr.RequisitionNumber;
                                requester = pr.RequesterName;
                            }
                        }
                        // يمكنك إضافة أنواع مستندات أخرى هنا في المستقبل

                        return new ApprovalRequestViewModel
                        {
                            ApprovalRequestId = ar.Id,
                            DocumentType = ar.DocumentType.ToString(),
                            DocumentNumber = docNumber,
                            RequesterName = requester,
                            RequestDate = ar.RequestDate,
                            CurrentStepName = ar.CurrentStep.StepName,
                            Status = ar.Status
                        };
                    }).ToList();

                    ApprovalsDataGrid.ItemsSource = viewModels;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل الموافقات المعلقة: {ex.Message}", "خطأ");
            }
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is ApprovalRequestViewModel vm)
            {
                ProcessApproval(vm.ApprovalRequestId, true);
            }
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is ApprovalRequestViewModel vm)
            {
                // يمكنك هنا فتح نافذة صغيرة لإدخال سبب الرفض
                ProcessApproval(vm.ApprovalRequestId, false, "مرفوض من قبل المدير");
            }
        }

        private void ProcessApproval(int approvalRequestId, bool isApproved, string reason = null)
        {
            using (var db = new DatabaseContext())
            {
                var request = db.ApprovalRequests.Include(ar => ar.CurrentStep).FirstOrDefault(ar => ar.Id == approvalRequestId);
                if (request == null) return;

                if (isApproved)
                {
                    // البحث عن الخطوة التالية
                    var nextStep = db.ApprovalWorkflowSteps
                        .Where(s => s.ApprovalWorkflowId == request.CurrentStep.ApprovalWorkflowId && s.StepOrder > request.CurrentStep.StepOrder)
                        .OrderBy(s => s.StepOrder)
                        .FirstOrDefault();

                    if (nextStep != null) // هناك خطوة تالية
                    {
                        request.CurrentStepId = nextStep.Id;
                    }
                    else // هذه هي الموافقة النهائية
                    {
                        request.Status = ApprovalStatus.Approved;
                        UpdateDocumentStatus(db, request.DocumentType, request.DocumentId, true);
                    }
                }
                else // تم الرفض
                {
                    request.Status = ApprovalStatus.Rejected;
                    request.RejectionReason = reason;
                    UpdateDocumentStatus(db, request.DocumentType, request.DocumentId, false);
                }

                request.LastActionDate = DateTime.UtcNow;
                db.SaveChanges();
                LoadPendingApprovals(); // تحديث القائمة
            }
        }

        private void UpdateDocumentStatus(DatabaseContext db, DocumentType docType, int docId, bool isApproved)
        {
            if (docType == DocumentType.PurchaseRequisition)
            {
                var pr = db.PurchaseRequisitions.Find(docId);
                if (pr != null)
                {
                    pr.Status = isApproved ? RequisitionStatus.Approved : RequisitionStatus.Rejected;
                }
            }
            // يمكنك إضافة أنواع مستندات أخرى هنا
        }
    }
}
