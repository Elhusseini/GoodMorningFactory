   M S B A M L   `   `   `  ����M  IGoodMorningFactory, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:GoodMorningFactory.UI.ViewsGoodMorningFactory.UI.Views  5      S.clr-namespace:GoodMorningFactory.UI.Converters GoodMorningFactory.UI.Converters  5	      /    )GoodMorningFactory.UI.Views.ShipmentsView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ;local)clr-namespace:GoodMorningFactory.UI.Views     5      E
converters.clr-namespace:GoodMorningFactory.UI.Converters     5	      $��RightToLeft=�5   8     � 	Resources  5      A   ;GoodMorningFactory.UI.Converters.EnumToDescriptionConverter   EnumToDescriptionConverter  TitleTextBlockStyle �� 
TargetType �� Property �� Value  FilterLabelStyle��%�  &	        &	      '�� �     &	       5   
   60   �� 5   
   " ��6,   .�5      ��  � ��$ 24(�6)   �� 5       � ��$
 Bold"�6+   �� 5       � ��$ 0,0,0,10q�6'   5      �� 5   
   " ��6   .�5      ��  � ��$	 8,4q�6(   �� 5       � ��$ 2q�6'   5      �� 5   
   " ��6)   .�5      ��  � ��$ Center=�62   �� 5       � ��$ 10,0,5,0q�6'   �� 5       � ��$ SemiBold"�6+   5      5      .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5       $
��Auto �6   �� 5!      $��* �6   .+�5%   
   �� $��0��6   $��إدارة الشحنات��6!   #��[ 66   �� 5(   
   $��1��6   
�������$��0,0,0,1q�66    �� Padding$ 0,5,0,10q�6P   $��0,0,0,10q�6c   ._�5)      � $��Center=�6   .+�5*      ��  �� Orientation$ 
Horizontal=�6   $	��5,2q�66   .+�5+      �� $��بحث:��6    #��[ 6,   �� 5,      -   5         ��Name$ SearchTextBox��5,      $	��250��65   
 � Tag$O Iابحث برقم الشحنة، أمر البيع، أو العميل...��6V   5-      �� 5/      $ 
Horizontal=�6   $	��5,2q�66   .+�50      �� $��الحالة:��6    #��[ 6/   �� 51      -   5        	 ��Name$	 StatusFilterComboBox��51      $	��150��6=   6I   52      �� 54      $ 
Horizontal=�6   $	��5,2q�66   .+�55      �� $��تاريخ الشحن:��6    #��[ 64   (  "System.Windows.Controls.DatePicker  56      -   5        
  Name$
 FromDatePicker��56   !   $	��120��69   6E   �� 57      $��-��6    $��Center=�6)   $	��5,0q�6D     58      -   5        $
 ToDatePicker��58   !   $	��120��67   6C   59      5:      5;      &   System.Windows.Controls.DataGrid  5>   
   -   5          Name$ ShipmentsDataGrid��5>      $��2��6.      AutoGenerateColumns$ False��6;      CanUserAddRows$ False��6W      
IsReadOnly$
 True��6n      Columns 5?      0  *System.Windows.Controls.DataGridTextColumn  5@      ,  &System.Windows.Controls.DataGridColumn   Header$ رقم الشحنة��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 6\   1  +System.Windows.Controls.DataGridBoundColumn   Binding 69   �� *ShipmentNumber+6\     5A      $ تاريخ الشحن��6%   $ * 6v    6:   �� *ShipmentDate+ �� StringFormat$ 
yyyy/MM/dd��6v     5B      $ أمر البيع��6%   $ * 6h    68   �� *SalesOrder.SalesOrderNumber+6h     5C      $ العميل��6%   $ 2* 6j    65   �� *" SalesOrder.Customer.CustomerName+6j     5D      $ الناقل��6%   $ * 6Q    65   �� *	Carrier+6Q     5E      $ رقم التتبع��6%   $ * 6\    69   �� *TrackingNumber+6\     5F      $ الحالة��6%   $ * 6�    65   �� *Status+ �� 	Converter# [  6�   4  .System.Windows.Controls.DataGridTemplateColumn  5G      $ إجراءات��6)   $
 Auto 6:      CellTemplate 5H      �� 5I      .R�5J      �� $ 
Horizontal=�6)   .+�5K   "   �� -   5        $��✏️��5K   )    � ToolTip$( "تعديل بيانات الشحن��6O   �� 5L   "   -   5        $
��📦��5L   )   $( "طباعة بوليصة الشحن��6U   5M      5N      5P      5R      5S      5T      