   M S B A M L   `   `   `  ����M  IGoodMorningFactory, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullS.clr-namespace:GoodMorningFactory.UI.Converters GoodMorningFactory.UI.Converters  5   	   7    1GoodMorningFactory.UI.Views.AddGoodsReceiptWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   E
converters.clr-namespace:GoodMorningFactory.UI.Converters     5   	     � Title$(  "تسجيل استلام بضاعة��5
   	   $	��600��6$   $	��950��61   $��RightToLeft=�5   	    � WindowStartupLocation$ CenterScreen=�6%    � 	Resources 5      C   =GoodMorningFactory.UI.Converters.BooleanToVisibilityConverter    BooleanToVisibilityConverter��%   &	          5      6,   5      .��5      � $��15q�6    � RowDefinitions 5      �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $��0,0,0,15q�6"   .+�5      �� $(��"تسجيل استلام بضاعة��6     TitleTextBlockStyle#���  62   �� 5      -   5         ��Name$" PurchaseOrderNumberTextBlock��5      $��16(�6>   5      &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ ReceiptItemsDataGrid��5      $��1��61      AutoGenerateColumns$ False��6>      CanUserAddRows$ False��6Z      Columns 5       0  *System.Windows.Controls.DataGridTextColumn  5!      ,  &System.Windows.Controls.DataGridColumn	   Header$	 اسم المنتج��6%   
   
IsReadOnly$

 True��6Y      Width5  /System.Windows.Controls.DataGridLengthConverter$ 2* 6k   1  +System.Windows.Controls.DataGridBoundColumn   Binding 69   �� *ProductName+6k     5"      $#	 الكمية المطلوبة��6%   $

 True��6b   $ * 6t    6>   �� *OrderedQuantity+6t     5#      $&	  تم استلامه سابقاً��6%   $

 True��6o   $ * 6�    6@   �� *PreviouslyReceivedQuantity+6�     5$      $,	 &الكمية المستلمة الآن��6%   $ * 6�    6C   �� *QuantityReceived+ �� UpdateSourceTrigger$ PropertyChanged=�6�   4  .System.Windows.Controls.DataGridTemplateColumn  5&      $	 بيانات التتبع��6)   $
 Auto 6@      CellTemplate 5'      �� 5(      .R�5)      �� -   5        $��إدخال الأرقام��5)   %   y�5+   %   �� *	IsTracked+ �� 	Converter# [  5,      5.        50      $"	 تخزين في الموقع��6)   $
 1.5* 6B    51      �� 52      .R�53      ��  �� DisplayMemberPath$
 Name��54   '    �� SelectedValuePath$ Id��55   '   ��53   '   �� *AvailableLocations+ �� SelectedValue 56   '   �� *DestinationLocationId+$ PropertyChanged=�57      59      5=      �� 5?   
   $��2��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   $��0,15,0,0q�6W   .+�5@      �� -   5        $!��تأكيد الاستلام��5@        PrimaryButtonStyle#���  6.   6[   �� 5A      $��
إلغاء��6     SecondaryButtonStyle#���  6%   $��10,0,0,0q�6T    �� IsCancel$
 True��6f   5B      5C      5D      