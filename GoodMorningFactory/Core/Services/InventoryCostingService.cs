// GoodMorning/Core/Services/InventoryCostingService.cs
// *** ملف جديد: خدمة مركزية لحساب تكلفة المخزون ***
using GoodMorningFactory.Data;
using GoodMorningFactory.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace GoodMorningFactory.Core.Services
{
    public static class InventoryCostingService
    {
        /// <summary>
        /// يحسب تكلفة البضاعة المصروفة بناءً على طريقة تقييم المخزون المحددة في الإعدادات.
        /// </summary>
        /// <param name="db">سياق قاعدة البيانات الحالي.</param>
        /// <param name="productId">معرف المنتج.</param>
        /// <param name="quantity">الكمية المصروفة.</param>
        /// <returns>إجمالي تكلفة الكمية المصروفة.</returns>
        public static decimal GetCostOfGoodsSold(DatabaseContext db, int productId, int quantity)
        {
            var companyInfo = db.CompanyInfos.FirstOrDefault();
            var costingMethod = companyInfo?.DefaultCostingMethod ?? InventoryCostingMethod.WeightedAverage;

            switch (costingMethod)
            {
                case InventoryCostingMethod.WeightedAverage:
                    return CalculateWacCost(db, productId, quantity);

                case InventoryCostingMethod.FIFO:
                    // سيتم تنفيذ منطق FIFO هنا في المستقبل
                    // حالياً، سنستخدم المتوسط المرجح كطريقة احتياطية
                    // throw new NotImplementedException("طريقة تقييم المخزون FIFO لم يتم تنفيذها بعد.");
                    return CalculateWacCost(db, productId, quantity);

                default:
                    return CalculateWacCost(db, productId, quantity);
            }
        }

        /// <summary>
        /// يحسب التكلفة باستخدام طريقة المتوسط المرجح.
        /// </summary>
        private static decimal CalculateWacCost(DatabaseContext db, int productId, int quantity)
        {
            var product = db.Products.Find(productId);
            if (product == null)
            {
                // إرجاع صفر إذا لم يتم العثور على المنتج لتجنب الأخطاء
                return 0;
            }
            return quantity * product.AverageCost;
        }
    }
}
