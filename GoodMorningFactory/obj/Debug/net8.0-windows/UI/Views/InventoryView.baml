   M S B A M L   `   `   `  ����  GoodMorningFactory/    )GoodMorningFactory.UI.Views.InventoryView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      $��RightToLeft=�5
      .��5      � $��10q�6     � RowDefinitions  5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6      FilterBorderStyle#���   6   ._�5      � .+�5      ��  �� Orientation$ 
Horizontal=�6   $��5q�66   .+�5      �� $��بحث:��6   $��Center=�6+   �� 5      -   5         ��Name$ SearchTextBox��5      $	��200��65   6A   5      �� 5      $ 
Horizontal=�6   $��5q�66   .+�5      �� $"��المخزن الرئيسي:��6   $��Center=�66   �� 5      -   5         ��Name$ WarehouseFilterComboBox��5      $	��150��6@    �� DisplayMemberPath$
 Name��6L    �� SelectedValuePath$ Id��6e   6|   5      �� 5      $ 
Horizontal=�6   $��5q�66   .+�5      �� $��الفئة:��6   $��Center=�6-   �� 5      -   5        $ CategoryFilterComboBox��5      $	��150��6?   $
 Name��6K   $ Id��6d   6{   5      �� 5       $ 
Horizontal=�6   $��5q�66   .+�5!      �� $��الحالة:��6   $��Center=�6.   �� 5"      -   5        $ StatusFilterComboBox��5"      $	��120��6=    �� ItemStringFormat 6I   ��   EnumToDescriptionConverter �� 	Converter# [ 6�   5#      5$      5%      &   System.Windows.Controls.DataGrid  5'   
   -   5          Name$ InventoryDataGrid��5'      $��1��6.   $��0,10,0,0q�6;   	   
IsReadOnly$
	 True��6M   
   AutoGenerateColumns$
 False��6_      Columns 5(      0  *System.Windows.Controls.DataGridTextColumn  5)      ,  &System.Windows.Controls.DataGridColumn   Header$ كود المنتج��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 6Y   1  +System.Windows.Controls.DataGridBoundColumn   Binding 69   �� *ProductCode+6Y     5*      $ اسم المنتج��6%   $ 2* 6Y    69   �� *ProductName+6Y     5+      $! المخزن الرئيسي��6%   $ * 6_    6=   �� *WarehouseName+6_     5,      $ الموقع الفرعي��6%   $ * 6d    6<   �� *StorageLocationName+6d     5-      $! الكمية المتاحة��6%   $ * 6`    6=   �� *QuantityOnHand+6`     5.      $# الكمية المحجوزة��6%   $ * 6c    6>   �� *QuantityReserved+6c     5/      $ إجمالي القيمة��6%   $ * 6i    6<   �� *TotalStockValueFormatted+6i     50      $ الحالة��6%   $ * 6�    65   �� *Status+# [ 6�   4  .System.Windows.Controls.DataGridTemplateColumn  51      $ إجراءات��6)   $
 Auto 6:      CellTemplate 52      �� 53      .R�54      �� $ 
Horizontal=�6)   .+�55   "   �� -   5        $��تعديل سريع��55   )     SecondaryButtonStyle#���  6^   $	��5,2q�6�   $��2q�6�   �� 56   "   -   5        $��سجل الحركات��56   )   #���  6_   $	��5,2q�6�   $��2q�6�   57      58      5:      5<      �� 5>   
   $��2��6   $ 
Horizontal=�6"   $��Right=�6;   $��0,10,0,0q�6W   .+�5?      �� -   5         ��Name$ PreviousPageButton��5?      $��السابق��61   6B   �� 5@      -	   5         ��Name$ PageInfoTextBlock��5@      $
��10,0q�63   $��Center=�6A   �� 5A      -
   5        $ NextPageButton��5A      $��التالي��6-   6>   5B      5C      5D      