   M S B A M L   `   `   `  ����M  IGoodMorningFactory, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:GoodMorningFactory.UI.ViewsGoodMorningFactory.UI.Views  5   	   >    8GoodMorningFactory.UI.Views.AddEditPurchaseInvoiceWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ;local)clr-namespace:GoodMorningFactory.UI.Views     5   	     � Title$&   إنشاء فاتورة شراء��5   	   $	��600��6#   $	��800��60    � WindowStartupLocation$ CenterScreen=�5	   	   $��RightToLeft=�6.   .��5
      � $��15q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6    �� Padding$ 10q�6   $��0,0,0,10q�6+   
�������$��1q�6U    �� CornerRadius$ 5��6i   ._�5      �  5      �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   �� 5      $
��Auto �6$    � ColumnDefinitions 5      �� 5      $
��Auto �6'   �� 5      $��* �6'   �� 5      $
��Auto �6'   �� 5      $��* �6'   .+�5!      �� $��0��6   $��0��6%   $��المورد:��65   �� 5"      -   5         ��Name$ SupplierComboBox��5"   8   $��0��6   $��1��6(    �� DisplayMemberPath$
 Name��6R    �� SelectedValuePath$ Id��6k   $��5q�6�   ${�False��6�   �� 5$      $��0��6   $��2��6%   $'��!رقم فاتورة المورد:��65   �� 5%      -   5        	 ��Name$	 InvoiceNumberTextBox��5%   7   $��0��6   $��3��6'   $��5q�6U   �� 5'      $��1��6   $��0��6%   $"��تاريخ الفاتورة:��65   (  "System.Windows.Controls.DatePicker  5(      -   5        
  Name$
 InvoiceDatePicker��5(   :   $��1��6   $��1��6*   $��5q�6U   �� 5*      $��1��6   $��2��6%   $$��تاريخ الاستحقاق:��65     5+      -   5        $
 DueDatePicker��5+   :   $��1��6   $��3��6*   $��5q�6Q   5,      5-      &   System.Windows.Controls.DataGrid  50   
   -   5          Name$ InvoiceItemsDataGrid��50       $��1��6      AutoGenerateColumns$ False��6>      CanUserAddRows$ False��6Z      
IsReadOnly$
 True��6q      Columns 51      0  *System.Windows.Controls.DataGridTextColumn  52      ,  &System.Windows.Controls.DataGridColumn   Header$ المنتج/المادة��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ 3* 6\   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6<   �� *ProductName+6\     53      $ الكمية��6%   $ * 6R    65   �� *
Quantity+6R     54      $ سعر الوحدة��6%   $ * 6h    69   �� *	UnitPrice+ �� StringFormat$ N2��6h     55      $# الإجمالي الفرعي��6%   $ * 6l      
FontWeight$
 Bold"�6v    6>   �� *
Subtotal+$ N2��6v   57      �� 5:   
   $��2��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   $��
0,10,10,10q�6W   .+�5;      �� $$��إجمالي الفاتورة:��6   $��18(�60   $
��Bold"�6>   $��Center=�6P   �� 5<      -   5         ��Name$ TotalAmountTextBlock��5<      $
��0.00��66   $��18(�6B   $
��Bold"�6P   ���#3182CE$
��10,0q�6w   5=      �� 5@   
   $��3��6   $ 
Horizontal=�6"   $
��Left=�6;   $��0,10,0,0q�6V   .+�5A      �� -   5         ��Name$ 
SaveButton��5A      $��حفظ الفاتورة��6)   $	��120��6@   $��5q�6L   6W   �� 5B      $��
إلغاء��6   $	��100��6%   $��5q�61    �� IsCancel$
 True��6<   5C      5D      5E      