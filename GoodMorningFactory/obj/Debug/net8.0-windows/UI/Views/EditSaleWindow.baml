   M S B A M L   `   `   `  ����  GoodMorningFactory0    *GoodMorningFactory.UI.Views.EditSaleWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     � Title$$  تعديل فاتورة بيع��5	   	   $	��700��6"   $
��1100��6/    � WindowStartupLocation$ CenterOwner=�5
   	   $��RightToLeft=�6-   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $%��معلومات الفاتورة��6   $��0��6-   $��10q�6:   .��5      �  � ColumnDefinitions 5      �� 5      $
��Auto �6'   �� 5      $��* �6'   �� 5      $
��Auto �6'   �� 5      $��* �6'   .+�5      �� $��0��6   $��العميل:��6(   �� 5      -   5         ��Name$ CustomerComboBox��5      $��1��65   $��5q�6E    �� DisplayMemberPath$ CustomerName��6P    �� SelectedValuePath$ Id��6q   ${�False��6�   �� 5      $��2��6   $��التاريخ:��6(   (  "System.Windows.Controls.DatePicker  5      -   5          Name$ SaleDatePicker��5      $��3��65   $��5q�6E   5      5      �� 5!   
   $��بنود الفاتورة��6   $��1��6*   $��
10,0,10,10q�67   .��5"      �  5#      �� 5$      $
��Auto �6$   �� 5%      $��* �6$   .+�5'      ��  �� Orientation$ 
Horizontal=�6   $��5q�66   .+�5(      �� $��بحث عن منتج:��6    $��Center=�64   $��5q�6O   �� 5)      -   5        	 ��Name$	 SearchProductTextBox��5)      $	��300��6<   6H   5*      &   System.Windows.Controls.DataGrid  5,      -   5        
  Name$
 SaleItemsDataGrid��5,      $��1��66   $��5q�6C      CanUserAddRows$ False��6N      AutoGenerateColumns$ False��6e      Columns 5-      0  *System.Windows.Controls.DataGridTextColumn  5.      ,  &System.Windows.Controls.DataGridColumn   Header$ المنتج��6-      Width5  /System.Windows.Controls.DataGridLengthConverter$ 3* 6]      
IsReadOnly$
 True��6h   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6=   �� *ProductName+6h     5/      $ سعر الوحدة��6-   $ * 6p    6A   �� *	UnitPrice+ �� StringFormat$ N2��6p     50      $ الكمية��6-   $ * 6Z    6=   �� *
Quantity+6Z     51      $ الإجمالي��6-   $ * 6m   $
 True��6w    6?   �� *
Subtotal+$ N2��6w   4  .System.Windows.Controls.DataGridTemplateColumn  52         CellTemplate 53      �� 54   "   .R�55   &   �� -   5        $	��❌��55   -   
������ $��0q�6k   
���  ��6   56   #   58      5:      5;      5<      �� 5>   
   $��2��6   $ 
Horizontal=�6"   $��Right=�6;   $��10q�6W   .+�5?      �� $��الإجمالي:��6   $��16(�6)   $
��Bold"�67   $��Center=�6I   �� 5@      -   5         ��Name$ TotalAmountTextBlock��5@      $
��0.00��66   $��16(�6B   $
��Bold"�6P   
����  �$��	10,0,20,0q�6t   $��Center=�6�   �� 5A      $"��المبلغ المدفوع:��6   $��16(�6/   $
��Bold"�6=   $��Center=�6O   �� 5B      -   5        $	 AmountPaidTextBox��5B      $	��100��61   $��5,0,20,0q�6=   $��Center=�6O   �� 5C      -   5        $$��💾 حفظ التعديلات��5C      $	��120��60   $��20,0,5,0q�6<   $��8q�6N   6Z   �� 5D      $��
إلغاء��6   $	��100��6%   $��5q�61   $��8q�6<    �� IsCancel$
 True��6H   5E      5F      5G      