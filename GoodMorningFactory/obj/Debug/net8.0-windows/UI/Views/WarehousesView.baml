   M S B A M L   `   `   `  ����  GoodMorningFactory0    *GoodMorningFactory.UI.Views.WarehousesView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      $��RightToLeft=�5
      .��5      � $��10q�6     � RowDefinitions  5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� $?��9إدارة المخازن والمواقع الفرعية��6      TitleTextBlockStyle#���   6:   $��0��6h   � 5   
   $��1��6    � ColumnDefinitions 5      �� 5      $	��300 �6#   �� 5      $��* �6#   .+�5      �� $%��المخازن الرئيسية��6   $��0��61   .��5      �   5      �� 5      $
��Auto �6(   �� 5      $��* �6(   .+�5       �� -   5         ��Name$ AddWarehouseButton��5       $"��إضافة مخزن جديد��69     PrimaryButtonStyle#���  6S   $��5q�6�   6�   &   System.Windows.Controls.DataGrid  5!      -   5          Name$ WarehousesDataGrid��5!      $��1��6;   $��5q�6H      AutoGenerateColumns$ False��6S      CanUserAddRows$ False��6o      
IsReadOnly$
 True��6�      Columns 5"      0  *System.Windows.Controls.DataGridTextColumn  5#      ,  &System.Windows.Controls.DataGridColumn   Header$ اسم المخزن��61   	   Width5  /System.Windows.Controls.DataGridLengthConverter$	 * 6^   1  +System.Windows.Controls.DataGridBoundColumn
   Binding
 6E   �� *Name+6^   4  .System.Windows.Controls.DataGridTemplateColumn  5$      $
	 Auto 65      CellTemplate 5%   "   �� 5&   &   .R�5'   *   �� -   5        $��
تعديل��5'   1     SecondaryButtonStyle#���  6c   $	��5,2q�6�   5(   '   5*      5,      5-      5.      �� 51      -   5         ��Name$ LocationsGroupBox��51      $#��المواقع الفرعية��62   $��1��6K   $��10,0,0,0q�6[   ${�False��6m   .��52      �   53      �� 54      $
��Auto �6(   �� 55      $��* �6(   .+�57      �� -   5        $ AddLocationButton��57      $"��إضافة موقع جديد��68   #���  6R   $��5q�6   $
��Left=�6�   6�     58      -   5        $ LocationsDataGrid��58      $��1��6:   $��5q�6G   $ False��6R   $ False��6n   $
 True��6�    59        5:      $ كود الموقع��61   $	 * 6^   
 6E   �� *Code+6^     5;      $ اسم الموقع��61   $	 2* 6^   
 6E   �� *Name+6^   4  .System.Windows.Controls.DataGridCheckBoxColumn  5<      $ نشط��65   $
	 Auto 6_   
 6B   �� *
IsActive+6_     5=      $ إجراءات��65   $
	 Auto 6F    5>   "   �� 5?   &   .R�5@   *   ��  �� Orientation$ 
Horizontal=�65   .+�5A   .   �� -   5        $��
تعديل��5A   5   $��5q�6E   #���  6q   $	��5,2q�6�   �� 5B   .   -   5        $��حذف��5B   5   $��5q�6C   #���  6q   $	��5,2q�6�   5C   +   5D   '   5F      5H      5I      5J      5K      5L      5M      