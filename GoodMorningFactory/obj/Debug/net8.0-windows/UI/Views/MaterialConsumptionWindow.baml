   M S B A M L   `   `   `  ����M  IGoodMorningFactory, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullS.clr-namespace:GoodMorningFactory.UI.Converters GoodMorningFactory.UI.Converters  5   	   ;    5GoodMorningFactory.UI.Views.MaterialConsumptionWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   E
converters.clr-namespace:GoodMorningFactory.UI.Converters     5   	     � Title$$  صرف مواد للإنتاج��5   	   $	��600��6"   $
��1000��6/   $��RightToLeft=�5	   	    � WindowStartupLocation$ CenterScreen=�6%    � 	Resources 5
          BooleanToVisibilityConverter��%   &	        �� 5   
   6'   5      .��5      � $��15q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $��0,0,0,15q�6"   .+�5      �� $$��صرف مواد للإنتاج��6     TitleTextBlockStyle#���  60   �� 5      -   5         ��Name$ WorkOrderNumberTextBlock��5      $��16(�6:   5      &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ MaterialsDataGrid��5      $��1��6.      AutoGenerateColumns$ False��6;      CanUserAddRows$ False��6W      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn	   Header$	 اسم المادة��6%   
   
IsReadOnly$

 True��6Z      Width5  /System.Windows.Controls.DataGridLengthConverter$ 2* 6l   1  +System.Windows.Controls.DataGridBoundColumn   Binding 69   �� *MaterialName+6l     5      $#	 الكمية المطلوبة��6%   $

 True��6t   $ * 6�    6>   �� *RequiredQuantity+ �� StringFormat$ N2��6�     5      $ 	 تم صرفه سابقاً��6%   $

 True��6}   $ * 6�    6=   �� *PreviouslyConsumedQuantity+$ N2��6�     5      $#	 الكمية المتبقية��6%   $

 True��6v   $ * 6�      
FontWeight$
 Bold"�6�    6>   �� *RemainingToConsume+$ N2��6�   4  .System.Windows.Controls.DataGridTemplateColumn  5      $	 صرف من الموقع��6)   $ 2* 6@      CellTemplate 5       �� 5!      .R�5"      ��  �� DisplayMemberPath$ DisplayName��5#   '    �� SelectedValuePath$ StorageLocationId��5$   '   ��5"   '   �� *AvailableLocations+ �� SelectedValue 5%   '   �� *SourceLocationId+ �� UpdateSourceTrigger$ PropertyChanged=�5&      5(        5)      $,	 &الكمية المصروفة الآن��6%   $ * 6�    6C   �� *ConsumedQuantity+$ PropertyChanged=�6�     5+      $	 بيانات التتبع��6)   $
 Auto 6@    5,      �� 5-      .R�5.      �� -   5        $!��اختيار الأرقام��5.   %   y�50   %   �� *	IsTracked+ �� 	Converter# [  51      53      55      �� 57   
   $��2��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   $��0,15,0,0q�6W   .+�58      �� -   5        $��تأكيد الصرف��58        PrimaryButtonStyle#���  6+   6X   �� 59      $��
إلغاء��6     SecondaryButtonStyle#���  6%   $��10,0,0,0q�6T    �� IsCancel$
 True��6f   5:      5;      5<      