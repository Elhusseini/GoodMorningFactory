// GoodMorningFactory/Core/Services/DepartmentService.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// خدمة مركزية لإدارة كل العمليات المتعلقة بالأقسام (CRUD Operations).
    /// هذا الكلاس هو المسؤول الوحيد عن التعامل المباشر مع جدول الأقسام في قاعدة البيانات.
    /// </summary>
    public class DepartmentService : IDepartmentService
    {
        public async Task<PaginatedResult<DepartmentViewModel>> GetDepartmentsAsync(DepartmentFilterCriteria criteria)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.Departments.AsQueryable();

                // تطبيق فلتر البحث إذا كان النص موجوداً
                if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                {
                    string searchTextLower = criteria.SearchText.ToLower();
                    query = query.Where(d => d.Name.ToLower().Contains(searchTextLower) || (d.Description != null && d.Description.ToLower().Contains(searchTextLower)));
                }

                var totalItems = await query.CountAsync();

                // تطبيق الترتيب والترقيم
                var departments = await query.OrderBy(d => d.Name)
                                             .Skip((criteria.Page - 1) * criteria.PageSize)
                                             .Take(criteria.PageSize)
                                             .Select(d => new DepartmentViewModel
                                             {
                                                 Id = d.Id,
                                                 Name = d.Name,
                                                 Description = d.Description,
                                                 CreatedAt = d.CreatedAt
                                             })
                                             .ToListAsync();

                return new PaginatedResult<DepartmentViewModel>
                {
                    Items = departments,
                    TotalCount = totalItems
                };
            }
        }

        public async Task<Department> GetDepartmentByIdAsync(int departmentId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Departments.FindAsync(departmentId);
            }
        }

        public async Task AddDepartmentAsync(Department department)
        {
            using (var db = new DatabaseContext())
            {
                db.Departments.Add(department);
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateDepartmentAsync(Department department)
        {
            using (var db = new DatabaseContext())
            {
                db.Departments.Update(department);
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteDepartmentAsync(int departmentId)
        {
            using (var db = new DatabaseContext())
            {
                // منع الحذف إذا كان القسم مرتبطاً بمستخدمين
                bool hasUsers = await db.Users.AnyAsync(u => u.DepartmentId == departmentId);
                if (hasUsers)
                {
                    throw new InvalidOperationException("لا يمكن حذف القسم لوجود مستخدمين مرتبطين به. يرجى نقل أو حذف المستخدمين أولاً.");
                }

                var departmentToDelete = await db.Departments.FindAsync(departmentId);
                if (departmentToDelete != null)
                {
                    db.Departments.Remove(departmentToDelete);
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task<int> GetNextDepartmentIdAsync()
        {
            using (var db = new DatabaseContext())
            {
                // إذا كان الجدول فارغاً، ابدأ من 1
                if (!await db.Departments.AnyAsync())
                    return 1;

                // وإلا، أوجد أعلى قيمة وأضف 1
                return await db.Departments.MaxAsync(d => d.Id) + 1;
            }
        }
    }
}
