   M S B A M L   `   `   `  ����M  IGoodMorningFactory, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:GoodMorningFactory.UI.ViewsGoodMorningFactory.UI.Views  5      S.clr-namespace:GoodMorningFactory.UI.Converters GoodMorningFactory.UI.Converters  5      0    *GoodMorningFactory.UI.Views.WorkOrdersView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ;local)clr-namespace:GoodMorningFactory.UI.Views     5      E
converters.clr-namespace:GoodMorningFactory.UI.Converters     5      ���#F4F7FC$��RightToLeft=�5      $��Segoe UI)�5        � 	Resources  5      A   ;GoodMorningFactory.UI.Converters.EnumToDescriptionConverter   EnumToDescriptionConverter �� 
TargetType  ActionButtonStyle �� Property �� Value �� BasedOn  StartButtonStyle  ConsumeButtonStyle  ReportButtonStyle  CancelButtonStyle��%�  &	        &	      &	 C    7  &	 �    7  &	 �    7  &	 <    7    5   
   60   �� 5   
   " ��6   .�5      ��  � �� �#4A55686+   �� 5       � ��
 �����6+   �� 5       � ��$	 8,4q�6(   �� 5       � ��$ 0q�60   �� 5       �@�Cursor$
 Hand��6'   �� 5       � ��$ 2q�6'   �� 5       � ��$ 11(�6)   5      �� 5   
   " ��6   8   .�5      ��  � �� �#38A1696+   5      �� 5   
   " ��6   8   .�5      ��  � �� �#DD6B206+   5       �� 5!   
   " ��6   8   .�5"      ��  � �� �#3182CE6+   5#      �� 5$   
   " ��6   8   .�5%      ��  � �� �#E53E3E6+   5&      5'      .��5)      � $��20q�6    � RowDefinitions 5*   
   �� 5+      $
��Auto �6   �� 5,      $
��Auto �6   �� 5-      $��* �6   �� 5.      $
��Auto �6   .+�51   
   �� $��0��6   $&�� إدارة أوامر العمل��6!   $��28(�6:   $
��Bold"�6H   ���#1A202C$��0,0,0,20q�6o   �� 54   
   $��1��6   
������� �� CornerRadius$ 8��61    �� Padding$ 15q�6B   $��0,0,0,15q�6O   ._�55      �  � ColumnDefinitions 56      �� 57      $��* �6'   �� 58      $
��Auto �6'   .+�5:      �� 	 �� Orientation$	 
Horizontal=�6   .+�5;      �� $��بحث:��6    $��Center=�6,   $��5q�6G   �� 5<      -   5        
 ��Name$
 SearchTextBox��5<      $	��200��65   $��5q�6A   6L   �� 5=      $��الحالة:��6    $��Center=�6/   $��10,0,0,0q�6J   �� 5>      -   5         ��Name$ StatusFilterComboBox��5>      $	��150��6=   $��5q�6I   ��5?      �� 5@      .R�5A   "   �� ��6,   ��  �� 	Converter# [  5B      5D      5E      �� 5F      -   5        $��1��5F      $'��!إضافة أمر عمل جديد��6)   ���#38A1696g   5G      5H      &   System.Windows.Controls.DataGrid  5K   
   -   5          Name$ WorkOrdersDataGrid��5K       $��2��6   $��0q�6<      
IsReadOnly$
 True��6G      AutoGenerateColumns$ False��6Y      Columns 5L      0  *System.Windows.Controls.DataGridTextColumn  5M      ,  &System.Windows.Controls.DataGridColumn   Header$ رقم الأمر��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 6\   1  +System.Windows.Controls.DataGridBoundColumn   Binding 68   �� *WorkOrderNumber+6\     5N      $ المنتج��6%   $ 2* 6[    65   �� *FinishedGood.Name+6[     5O      $# الكمية المطلوبة��6%   $ * 6d    6>   �� *QuantityToProduce+6d     5P      $! الكمية المنتجة��6%   $ * 6b    6=   �� *QuantityProduced+6b     5Q      $( "تاريخ البدء المخطط��6%   $ * 6v    6A   �� *PlannedStartDate+ �� StringFormat$ d��6v     5S      $ الحالة��6%   $ * 6�      
FontWeight$ SemiBold"�6�    65   �� *Status+# [  6�   4  .System.Windows.Controls.DataGridTemplateColumn  5U      $ إجراءات��6)   $
 Auto 6:      CellTemplate 5V      �� 5W      .R�5X      �� $	 
Horizontal=�6)   .+�5Z   "   �� -   5        $��عرض/تعديل��5Z   )   #��[ 6=   6h   �� 5[   "   -   5        $��بدء التنفيذ��5[   )   #��[ 6?   6i   �� 5\   "   -   5        $��صرف مواد��5\   )   #��[ 6<   6h   �� 5]   "   -   5        $��تسجيل إنتاج��5]   )   #��[ 6?   6j   �� 5^   "   -	   5        $��تسجيل عمالة��5^   )   #��[ 6?   6j   �� 5_   "   -
   5        $��
إلغاء��5_   )   #��[ 69   6d   5a      5b      5d      5f      �� 5i   
   $��3��6   $	 
Horizontal=�6"   $��Center=�6;   $��0,10,0,0q�6X   .+�5j      �� -   5         ��Name$ NextPageButton��5j      $��التالي ←��6-   $	��5,0q�6@   6M   �� 5k      -   5         ��Name$ PageInfoTextBlock��5k      $��Center=�63   $��SemiBold"�6N   �� 5l      -   5        $ PreviousPageButton��5l      $��→ السابق��61   $	��5,0q�6D   6Q   5m      5n      5o      