   M S B A M L   `   `   `  ����  GoodMorningFactory3    -GoodMorningFactory.UI.Views.AdjustStockWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$(  "جرد وتعديل المخزون��5   	   $	��600��6$   $	��900��61   $��RightToLeft=�5   	    � WindowStartupLocation$ CenterScreen=�6%   .��5      � $��15q�6    � RowDefinitions 5	   
   �� 5
      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $(��"جرد وتعديل المخزون��6      TitleTextBlockStyle#���   6.   $��0��6\   �� 5   
   $��1��6     FilterBorderStyle#���  6   ._�5      �  � ColumnDefinitions 5      �� 5      $
��Auto �6'   �� 5      $��* �6'   �� 5      $
��Auto �6'   �� 5      $��* �6'   �� 5      $
��Auto �6'   �� 5      $��* �6'   .+�5      �� $ ��تاريخ التعديل:��6   $��0��61   (  "System.Windows.Controls.DatePicker  5      -   5          Name$ AdjustmentDatePicker��5      $��1��6;   $��5q�6K   �� 5      $"��المخزن الرئيسي:��6   $��2��62   $��10,0,0,0q�6B   �� 5      -   5         ��Name$ WarehouseComboBox��5      $��3��66   $��5q�6F    �� DisplayMemberPath$
 Name��6Q    �� SelectedValuePath$ Id��6j   6�   �� 5!      $ ��الموقع الفرعي:��6   $��4��61   $��10,0,0,0q�6A   �� 5"      -   5        $ LocationComboBox��5"      $��5��65   $��5q�6E   $
 Name��6P   $ Id��6i   6�   5$      5%      �� 5'   
   $,��&الأصناف المراد جردها��6   $��2��61   $��0,10,0,0q�6>   .��5(      �  5)      �� 5*      $
��Auto �6$   �� 5+      $��* �6$   .+�5-      �� $��0��6    �� Orientation$ 
Horizontal=�6*   $��5q�6C   .+�5.      �� $:��4بحث عن منتج (بالكود أو الاسم):��6   �� 5/      -   5        	 ��Name$	 SearchProductTextBox��5/      $	��250��6<   ${�False��6k   50      &   System.Windows.Controls.DataGrid  51      -   5        
  Name$
 ItemsDataGrid��51      $��1��62      AutoGenerateColumns$ False��6?      CanUserAddRows$ False��6[      Columns 52      0  *System.Windows.Controls.DataGridTextColumn  53      ,  &System.Windows.Controls.DataGridColumn   Header$ كود المنتج��6-      
IsReadOnly$
 True��6a      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 6s   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6A   �� *ProductCode+6s     54      $ اسم المنتج��6-   $
 True��6a   $ 2* 6s    6A   �� *ProductName+6s     55      $! الكمية بالنظام��6-   $
 True��6h   $ * 6z    6E   �� *SystemQuantity+6z     56      $! الكمية الفعلية��6-   $ * 6�    6E   �� *ActualQuantity+ �� UpdateSourceTrigger$ PropertyChanged=�6�     57      $ 
الفرق��6-   $
 True��6[   $ * 6m    6<   �� *
Difference+6m     58      $ قيمة الفرق��6-   $
 True��6n   $ * 6�    6A   �� *DifferenceValueFormatted+6�   5:      5;      5<      �� 5>   
   $��3��6   $ 
Horizontal=�6"   $��Right=�6;   $��0,15,0,0q�6W   .+�5?      �� -   5        $��ترحيل التعديل��5?        PrimaryButtonStyle#���  6-   6Z   �� 5@      $��
إلغاء��6     SecondaryButtonStyle#���  6%   $��10,0,0,0q�6T    �� IsCancel$
 True��6f   5A      5B      5C      