// ...existing code...
                            // Create and save GRN
                            var grn1 = new GoodsReceiptNote
                            {
                                GRNNumber = $"GRN-{DateTime.Now:yyyyMMdd}001",
                                PurchaseOrderId = purchaseOrder1.Id,
                                ReceiptDate = DateTime.Today.AddDays(-35)
                            };

                            db.GoodsReceiptNotes.Add(grn1);
                            db.SaveChanges();

                            // Add GRN items separately
                            var grnItems = purchaseOrder1.PurchaseOrderItems.Select(poi => new GoodsReceiptNoteItem
                            {
                                GoodsReceiptNoteId = grn1.Id,
                                ProductId = poi.ProductId,
                                QuantityReceived = poi.Quantity
                            }).ToList();

                            db.GoodsReceiptNoteItems.AddRange(grnItems);
                            db.SaveChanges();

                            // Create and save purchase invoice
                            var purchase1 = new Purchase
                            {
                                InvoiceNumber = $"PI-{DateTime.Now:yyyyMMdd}001",
                                PurchaseOrderId = purchaseOrder1.Id,
                                SupplierId = supplier1.Id,
                                PurchaseDate = DateTime.Today.AddDays(-35),
                                DueDate = DateTime.Today.AddDays(-5),
                                Status = PurchaseInvoiceStatus.ApprovedForPayment,
                                TotalAmount = purchaseOrder1.TotalAmount,
                                AmountPaid = purchaseOrder1.TotalAmount
                            };

                            db.Purchases.Add(purchase1);
                            db.SaveChanges();

                            // Link the GRN to the purchase
                            purchase1.GoodsReceiptNotes.Add(grn1);
                            grn1.PurchaseId = purchase1.Id;
                            grn1.Purchase = purchase1;
                            db.SaveChanges();