// GoodMorningFactory/Core/Services/CategoryService.cs
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using GoodMorningFactory.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GoodMorningFactory.Core.Services
{
    /// <summary>
    /// خدمة مركزية لإدارة كل العمليات المتعلقة بالفئات.
    /// </summary>
    public class CategoryService : ICategoryService
    {
        public async Task<ObservableCollection<CategoryViewModel>> GetCategoryTreeAsync()
        {
            using (var db = new DatabaseContext())
            {
                // استخدام Include لجلب عدد المنتجات بكفاءة
                var categories = await db.Categories.Include(c => c.Products).ToListAsync();
                var categoryViewModels = categories.Select(c => new CategoryViewModel(c, c.Products.Count)).ToList();

                var dictionary = categoryViewModels.ToDictionary(vm => vm.Category.Id);
                var rootCategories = new ObservableCollection<CategoryViewModel>();

                foreach (var vm in categoryViewModels)
                {
                    if (vm.Category.ParentCategoryId.HasValue && dictionary.ContainsKey(vm.Category.ParentCategoryId.Value))
                    {
                        dictionary[vm.Category.ParentCategoryId.Value].Children.Add(vm);
                    }
                    else
                    {
                        rootCategories.Add(vm);
                    }
                }
                return rootCategories;
            }
        }

        public async Task<List<Category>> GetPossibleParentCategoriesAsync(int? currentCategoryId)
        {
            using (var db = new DatabaseContext())
            {
                var query = db.Categories.AsQueryable();
                // في حالة التعديل، استبعد الفئة نفسها من قائمة الآباء المحتملين
                if (currentCategoryId.HasValue)
                {
                    query = query.Where(c => c.Id != currentCategoryId.Value);
                }
                return await query.OrderBy(c => c.Name).ToListAsync();
            }
        }

        public async Task<Category> GetCategoryByIdAsync(int categoryId)
        {
            using (var db = new DatabaseContext())
            {
                return await db.Categories.FindAsync(categoryId);
            }
        }

        public async Task AddCategoryAsync(Category category)
        {
            using (var db = new DatabaseContext())
            {
                db.Categories.Add(category);
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            using (var db = new DatabaseContext())
            {
                db.Categories.Update(category);
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            using (var db = new DatabaseContext())
            {
                // التحقق من وجود فئات فرعية
                bool hasChildren = await db.Categories.AnyAsync(c => c.ParentCategoryId == categoryId);
                if (hasChildren)
                {
                    throw new InvalidOperationException("لا يمكن حذف الفئة لوجود فئات فرعية مرتبطة بها. يرجى حذف الفئات الفرعية أولاً.");
                }

                // التحقق من وجود منتجات مرتبطة
                bool hasProducts = await db.Products.AnyAsync(p => p.CategoryId == categoryId);
                if (hasProducts)
                {
                    throw new InvalidOperationException("لا يمكن حذف الفئة لوجود منتجات مرتبطة بها.");
                }

                var categoryToDelete = await db.Categories.FindAsync(categoryId);
                if (categoryToDelete != null)
                {
                    db.Categories.Remove(categoryToDelete);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
