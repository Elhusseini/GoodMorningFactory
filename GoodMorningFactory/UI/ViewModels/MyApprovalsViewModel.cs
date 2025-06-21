using GoodMorningFactory.Core.Services;
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using GoodMorningFactory.UI.Commands; // Ensure this is the correct path for your project structure

namespace GoodMorningFactory.UI.ViewModels
{
    public class MyApprovalsViewModel : ViewModelBase
    {
        private ObservableCollection<ApprovalRequestViewModel> _pendingApprovals;
        public ObservableCollection<ApprovalRequestViewModel> PendingApprovals
        {
            get => _pendingApprovals;
            set { _pendingApprovals = value; OnPropertyChanged(); }
        }

        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }

        public MyApprovalsViewModel()
        {
            ApproveCommand = new RelayCommand(ApproveRequest);
            RejectCommand = new RelayCommand(RejectRequest);
            LoadPendingApprovals();
        }

        private async void LoadPendingApprovals()
        {
            if (CurrentUserService.LoggedInUser == null) return;

            try
            {
                using (var db = new DatabaseContext())
                {
                    var userRoleId = CurrentUserService.LoggedInUser.RoleId;

                    // Optimized query to fetch all data in one go
                    var approvals = await db.ApprovalRequests
                        .Include(ar => ar.CurrentStep)
                        .Where(ar => ar.Status == ApprovalStatus.Pending && ar.CurrentStep.ApproverRoleId == userRoleId)
                        .Select(ar => new ApprovalRequestViewModel
                        {
                            ApprovalRequestId = ar.Id,
                            DocumentType = ar.DocumentType.ToString(),
                            RequestDate = ar.RequestDate,
                            CurrentStepName = ar.CurrentStep.StepName,
                            Status = ar.Status,
                            // Fetch related document info within the same query
                            DocumentNumber = (ar.DocumentType == DocumentType.PurchaseRequisition)
                                             ? db.PurchaseRequisitions.FirstOrDefault(pr => pr.Id == ar.DocumentId).RequisitionNumber
                                             : "N/A",
                            RequesterName = (ar.DocumentType == DocumentType.PurchaseRequisition)
                                            ? db.PurchaseRequisitions.FirstOrDefault(pr => pr.Id == ar.DocumentId).RequesterName
                                            : "N/A"
                        })
                        .ToListAsync();

                    PendingApprovals = new ObservableCollection<ApprovalRequestViewModel>(approvals);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"فشل تحميل الموافقات المعلقة: {ex.Message}", "خطأ");
            }
        }

        private void ApproveRequest(object parameter)
        {
            if (parameter is ApprovalRequestViewModel vm)
            {
                ProcessApproval(vm.ApprovalRequestId, true);
            }
        }

        private void RejectRequest(object parameter)
        {
            if (parameter is ApprovalRequestViewModel vm)
            {
                // For simplicity, we use a hardcoded reason. A real app would show an input dialog.
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
                    var nextStep = db.ApprovalWorkflowSteps
                        .Where(s => s.ApprovalWorkflowId == request.CurrentStep.ApprovalWorkflowId && s.StepOrder > request.CurrentStep.StepOrder)
                        .OrderBy(s => s.StepOrder)
                        .FirstOrDefault();

                    if (nextStep != null)
                    {
                        request.CurrentStepId = nextStep.Id;
                    }
                    else
                    {
                        request.Status = ApprovalStatus.Approved;
                        UpdateDocumentStatus(db, request.DocumentType, request.DocumentId, true);
                    }
                }
                else
                {
                    request.Status = ApprovalStatus.Rejected;
                    request.RejectionReason = reason;
                    UpdateDocumentStatus(db, request.DocumentType, request.DocumentId, false);
                }

                request.LastActionDate = DateTime.UtcNow;
                db.SaveChanges();
                LoadPendingApprovals(); // Refresh the list
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
        }
    }
}
