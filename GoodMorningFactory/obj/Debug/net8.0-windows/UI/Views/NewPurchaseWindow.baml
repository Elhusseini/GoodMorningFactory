   M S B A M L   `   `   `  ����  GoodMorningFactory3    -GoodMorningFactory.UI.Views.NewPurchaseWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     � Title$&   فاتورة شراء جديدة��5	   	   $	��650��6#   $
��1100��60    � WindowStartupLocation$ CenterOwner=�5
   	   $��RightToLeft=�6-   .��5      �  � ColumnDefinitions 5   
   �� 5      $��* �6   �� 5      $	��300 �6   .+�5   
   � $��0��6   $��10q�6    � RowDefinitions 5      �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $��* �6    .+�5      �� $%��معلومات الفاتورة��6   $��0��61   .��5      �  5      �� 5      $
��Auto �6+   �� 5      $��* �6+    5      �� 5      $
��Auto �6(   �� 5       $
��Auto �6(   .+�5#      �� $��0��6   $��0��6)   $��المورد:��69   �� 5$      -   5         ��Name$ SupplierComboBox��5$      $��0��69   $��1��6F   $��5q�6V    �� DisplayMemberPath$
 Name��6a    �� SelectedValuePath$ Id��6z   �� 5&      $��1��6   $��0��6)   $'��!رقم فاتورة المورد:��69   �� 5'      -   5         ��Name$ InvoiceNumberTextBox��5'      $��1��6<   $��1��6I   $��5q�6Y   5(      5)      �� 5+      $"��بحث وإضافة منتج��6   $��1��60   $��0,10,0,0q�6=   .��5,      ��  �� Orientation$ 
Horizontal=�6   .+�5-      �� $,��&ابحث بالاسم أو الكود:��6    $��Center=�6=   $��5q�6X   �� 5.      -   5        $ SearchProductTextBox��5.      $	��300��6<   $��5q�6H   6S   5/      50      &   System.Windows.Controls.DataGrid  52      -   5        	  Name$	 InvoiceItemsDataGrid��52      $��2��65   $��0,10,0,0q�6B   
   CanUserAddRows$
 False��53         AutoGenerateColumns$ False��6.      Columns 54      0  *System.Windows.Controls.DataGridTextColumn  55      ,  &System.Windows.Controls.DataGridColumn   Header$ المنتج��6)      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 6Y      
IsReadOnly$
 True��6c   1  +System.Windows.Controls.DataGridBoundColumn   Binding 69   �� *ProductName+6c     56      $ سعر الشراء��6)   $	 120 6q    6=   �� *	UnitPrice+ �� StringFormat$ {0:C}��6q     57      $ الكمية��6)   $ 80 6V    69   �� *
Quantity+6V     58      $# الإجمالي الفرعي��6)   $	 130 6u   $
 True��6�    6B   �� *
Subtotal+$ {0:C}��6�   4  .System.Windows.Controls.DataGridTemplateColumn  59      $ 50 6-      CellTemplate 5:      �� 5;      .R�5<   "   �� -   5        $	��❌��5<   )   
������ $��0q�6m   
���  ��6�   5=      5?      5A      5B      �� 5D   
   $��1��6   ���#F5F5F5 �� Padding$ 15q�66   ._�5E      �� .+�5F      �� $��ملخص الفاتورة��6   $��22(�61   $
��Bold"�6?   $��0,0,0,20q�6Q   �� 5G      � 5H      $
��0,10q�6    5I      �� 5J      $��* �6+   �� 5K      $��* �6+   .+�5M      �� $��0��6    $��الإجمالي��60   $��18(�6@   �� 5N      -   5         ��Name$ TotalAmountTextBlock��5N   0   $��1��6    $
��0.00��6N   $��18(�6Z   $
��Bold"�6h   $
��Left=�6z   5O      �� 5P      �� 5Q      -   5         ��Name$ CompletePurchaseButton��5Q      $%��✅ تسجيل الفاتورة��69   $��10q�6T   $��0,40,0,0q�6a   $��18(�6s   $
��Bold"�6�   
����·�6�   �� 5R      $��
إلغاء��6   $��8q�6)   $��0,10,0,0q�65    �� IsCancel$
 True��6G   5S      5T      5U      5V      