   M S B A M L   `   `   `  ����  GoodMorningFactory<    6GoodMorningFactory.UI.Views.AddEditPurchaseOrderWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     � Title$-  'إنشاء / تعديل أمر شراء��5      $	��600��69   $	��900��6F    � WindowStartupLocation$ CenterOwner=�5	   	   $��RightToLeft=�6-   .��5
      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $(��"معلومات أمر الشراء��6   $��0��6/   $��0,0,0,10q�6<   .��5      � $��	10,5,10,5q�6    5      �� 5      $
��Auto �6$   �� 5      $
��Auto �6$    � ColumnDefinitions 5      �� 5      $
��Auto �6'   �� 5      $��2* �6'   �� 5      $
��Auto �6'   �� 5      $��2* �6'   .+�5      �� $��0��6   $��0��6%   $��المورد:��65   $��Center=�6G   $��0,0,5,0q�6b   �� 5      -   5         ��Name$ SupplierComboBox��5      $��0��65   $��1��6B   $��0,2,10,2q�6R    �� DisplayMemberPath$
 Name��6d    �� SelectedValuePath$ Id��6}   �� 5       $��0��6   $��2��6%   $��تاريخ الأمر:��65   $��Center=�6L   $��0,0,5,0q�6g   (  "System.Windows.Controls.DatePicker  5!      -   5          Name$ OrderDatePicker��5!      $��0��66   $��3��6C   $��0,2,0,2q�6S   �� 5#      $��1��6   $��0��6%   $#��مرجع طلب الشراء:��65   $��Center=�6P   $��0,0,5,0q�6k   y�6|   ��  �� ElementName$ RequisitionRefTextBlock��	 �� Path$	 
Visibility��� 5$      -   5        
 ��Name$
 RequisitionRefTextBlock��5$      $��1��6=   $��1��6J   $
��Bold"�6Z   $��Center=�6l   $��0,2,10,2q�6�   $y�	Collapsed=�6�   5%      5&      &   System.Windows.Controls.DataGrid  5(   
   -   5          Name$ OrderItemsDataGrid��5(      $��1��6/   $��0,0,0,10q�6<      CanUserAddRows$
 True��6N      AutoGenerateColumns$ False��6d      Columns 5)      4  .System.Windows.Controls.DataGridComboBoxColumn  5*      -   5        ProductColumn��,  &System.Windows.Controls.DataGridColumn   Header$* $المنتج/المادة الخام��5*   @      SelectedValuePath$ Id��6�      DisplayMemberPath$
 Name��6�      Width5  /System.Windows.Controls.DataGridLengthConverter$ 3* 6�      SelectedValueBinding 6]   �� *	ProductId+6�   0  *System.Windows.Controls.DataGridTextColumn  5+      $ الكمية��6%   $ * 6R   1  +System.Windows.Controls.DataGridBoundColumn   Binding 65   �� *
Quantity+6R     5,      $ سعر الوحدة��6%   $ * 6h    69   �� *	UnitPrice+ �� StringFormat$ N2��6h     5-      $ 
الوصف��6%      
IsReadOnly$
 True��6T   $ 2* 6f      
Foreground
 ����� 64   �� *Description+6q   5/      �� 51   
   -   5        $��2��51      $��💾 حفظ��6   $
��Left=�6/   $
��15,5q�6J   6Y   52      53      