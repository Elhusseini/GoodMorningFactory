   M S B A M L   `   `   `  ����  GoodMorningFactory3    -GoodMorningFactory.UI.Views.AddShipmentWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$$  إنشاء شحنة جديدة��5   	   $	��600��6"   $	��950��6/   $��RightToLeft=�5   	    � WindowStartupLocation$ CenterScreen=�6%    � 	Resources 5          BooleanToVisibilityConverter��%   &	        �� 5   
   6'   5	      .��5      � $��15q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $$��إنشاء شحنة جديدة��6     TitleTextBlockStyle#���  6,   $��0��6Z   � 5   
   $��1��6    � ColumnDefinitions 5      �� 5      $��* �6#   �� 5      $��* �6#   .+�5      �� $��5q�6   .+�5      �� $��رقم أمر البيع:��6   �� 5      -   5         ��Name$ SalesOrderNumberTextBox��5       �� 
IsReadOnly$
 True��6;   5      �� 5      $��1��6   $��5q�6)   .+�5      �� $��تاريخ الشحن:��6   (  "System.Windows.Controls.DatePicker  5       -   5          Name$ ShipmentDatePicker��5       5!      5"      &   System.Windows.Controls.DataGrid  5$   
   -   5          Name$ ShipmentItemsDataGrid��5$      $��2��62   $��0,10,0,0q�6?   	   AutoGenerateColumns$	 False��6Q   
   CanUserAddRows$
 False��6m      Columns 5%      0  *System.Windows.Controls.DataGridTextColumn  5&      ,  &System.Windows.Controls.DataGridColumn   Header$ اسم المنتج��6%      
IsReadOnly$
 True��6Y      Width5  /System.Windows.Controls.DataGridLengthConverter$ 2* 6k   1  +System.Windows.Controls.DataGridBoundColumn   Binding 69   �� *ProductName+6k     5'      $# الكمية المطلوبة��6%   $
 True��6b   $ * 6t    6>   �� *OrderedQuantity+6t     5(      $  تم شحنه سابقاً��6%   $
 True��6k   $ * 6}    6=   �� *PreviouslyShippedQuantity+6}     5)      $* $الكمية المراد شحنها��6%   $ * 6�    6B   �� *QuantityToShip+ �� UpdateSourceTrigger$ PropertyChanged=�6�   4  .System.Windows.Controls.DataGridTemplateColumn  5*      $ صرف من الموقع��6)   $
 1.5* 6@      CellTemplate 5+      �� 5,      .R�5-      ��  �� DisplayMemberPath$ DisplayName��5.   '    �� SelectedValuePath$ StorageLocationId��5/   '   ��5-   '   �� *AvailableLocations+ �� SelectedValue 50   '   �� *SourceLocationId+$ PropertyChanged=�51      53        55      $ بيانات التتبع��6)   $
 Auto 6@    56      �� 57      .R�58      �� -   5        $!��اختيار الأرقام��58   %   y�5:   %   �� *	IsTracked+ �� 	Converter# [  5;      5=      5A      �� 5C   
   $��3��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   $��0,15,0,0q�6W   .+�5D      �� -   5        $,��&تأكيد الشحن والفوترة��5D        PrimaryButtonStyle#���  64   6a   �� 5E      $��
إلغاء��6     SecondaryButtonStyle#���  6%   $��10,0,0,0q�6T    �� IsCancel$
 True��6f   5F      5G      5H      