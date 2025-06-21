// GoodMorningFactory/Core/Services/PurchaseRequisitionService.cs
// *** الكود الكامل والنهائي (لإصلاح خطأ SQLite) ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GoodMorningFactory.Core.Services
{
    public class PurchaseRequisitionService : IPurchaseRequisitionService
    {
        public async Task<PagedResult<PurchaseRequisition>> GetRequisitionsAsync(int page, int pageSize, string searchText, RequisitionStatus? status)
        {
            using (var db = new DatabaseContext()) { var query = db.PurchaseRequisitions.AsQueryable(); if (!string.IsNullOrWhiteSpace(searchText)) { string searchTextLower = searchText.ToLower(); query = query.Where(pr => pr.RequisitionNumber.ToLower().Contains(searchTextLower) || pr.RequesterName.ToLower().Contains(searchTextLower)); } if (status.HasValue) { query = query.Where(pr => pr.Status == status.Value); } int totalItems = await query.CountAsync(); var results = await query.OrderByDescending(pr => pr.RequisitionDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(); return new PagedResult<PurchaseRequisition> { Items = results, TotalItems = totalItems }; }
        }

        public async Task<PurchaseRequisition> GetRequisitionByIdAsync(int requisitionId)
        {
            using (var db = new DatabaseContext()) { return await db.PurchaseRequisitions.Include(pr => pr.PurchaseRequisitionItems).FirstOrDefaultAsync(pr => pr.Id == requisitionId); }
        }

        public async Task SaveRequisitionAsync(PurchaseRequisition requisition)
        {
            using (var db = new DatabaseContext()) { if (requisition.Id == 0) { db.PurchaseRequisitions.Add(requisition); } else { var existing = await db.PurchaseRequisitions.Include(pr => pr.PurchaseRequisitionItems).FirstOrDefaultAsync(p => p.Id == requisition.Id); if (existing != null) { db.Entry(existing).CurrentValues.SetValues(requisition); db.PurchaseRequisitionItems.RemoveRange(existing.PurchaseRequisitionItems); foreach (var item in requisition.PurchaseRequisitionItems) { existing.PurchaseRequisitionItems.Add(item); } } } await db.SaveChangesAsync(); }
        }

        public async Task SubmitForApprovalAsync(int requisitionId)
        {
            using (var db = new DatabaseContext())
            {
                var prToUpdate = await db.PurchaseRequisitions.Include(pr => pr.PurchaseRequisitionItems).ThenInclude(i => i.Product).FirstOrDefaultAsync(pr => pr.Id == requisitionId);
                if (prToUpdate == null) return;
                decimal totalAmount = prToUpdate.PurchaseRequisitionItems.Sum(item => item.Quantity * (item.Product?.PurchasePrice ?? 0));
                prToUpdate.TotalAmount = totalAmount;
                var potentialWorkflows = await db.ApprovalWorkflows.Include(aw => aw.Steps).Where(aw => aw.DocumentType == DocumentType.PurchaseRequisition && aw.IsActive && totalAmount >= aw.MinimumAmount).ToListAsync();
                var approvalWorkflow = potentialWorkflows.OrderByDescending(aw => aw.MinimumAmount).FirstOrDefault();
                if (approvalWorkflow != null && approvalWorkflow.Steps.Any())
                {
                    var firstStep = approvalWorkflow.Steps.OrderBy(s => s.StepOrder).First();
                    var existingRequest = await db.ApprovalRequests.FirstOrDefaultAsync(ar => ar.DocumentId == prToUpdate.Id && ar.DocumentType == DocumentType.PurchaseRequisition);
                    if (existingRequest != null) { db.ApprovalRequests.Remove(existingRequest); }
                    var newApprovalRequest = new ApprovalRequest { DocumentId = prToUpdate.Id, DocumentType = DocumentType.PurchaseRequisition, CurrentStepId = firstStep.Id, Status = ApprovalStatus.Pending, RequestDate = DateTime.Now, CurrentApproverRoleId = firstStep.ApproverRoleId, ApprovalWorkflowId = approvalWorkflow.Id, RejectionReason = "" };
                    db.ApprovalRequests.Add(newApprovalRequest);
                    prToUpdate.Status = RequisitionStatus.PendingApproval;
                }
                else
                {
                    prToUpdate.Status = RequisitionStatus.Approved;
                }
                await db.SaveChangesAsync();
            }
        }
    }
}