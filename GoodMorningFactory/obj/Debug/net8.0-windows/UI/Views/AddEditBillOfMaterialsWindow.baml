   M S B A M L   `   `   `  ����M  IGoodMorningFactory, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:GoodMorningFactory.UI.ViewsGoodMorningFactory.UI.Views  5   	   >    8GoodMorningFactory.UI.Views.AddEditBillOfMaterialsWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ;local)clr-namespace:GoodMorningFactory.UI.Views     5   	     � Title$5  /إنشاء / تعديل قائمة مكونات��5   	   $	��600��6,   $	��800��69    � WindowStartupLocation$ CenterScreen=�5	   	   $��RightToLeft=�5
   	   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $��10q�6"   .+�5      �� $"��المنتج النهائي:��6   $��SemiBold"�6/   �� 5      -   5         ��Name$ FinishedGoodComboBox��5       �� DisplayMemberPath$
 Name��65    �� SelectedValuePath$ Id��6N   $��0,0,0,10q�6e   �� 5      $��الوصف:��6   $��SemiBold"�6&   �� 5      -   5         ��Name$ DescriptionTextBox��5      5      � 5   
   $��1��6   $��10q�6    5      �� 5      $
��Auto �6    �� 5      $��* �6    .+�5      ]� $��0��6   $��0,0,0,10q�6%   .+�5       �� $"��بحث عن مادة خام:��6   $��Center=�64   �� 5!      -   5         ��Name$ AddMaterialButton��5!      $��
إضافة��64   $
��Left=�6D   $��10,0,0,0q�6Z   6l   �� 5"      -   5        $ SearchMaterialTextBox��5"      $
��10,0q�69   6G   5#      &   System.Windows.Controls.DataGrid  5%      -   5          Name$ BomItemsDataGrid��5%   $   $��1��6   	   AutoGenerateColumns$	 False��6>   
   CanUserAddRows$
 False��6Z      Columns 5&      0  *System.Windows.Controls.DataGridTextColumn  5'      ,  &System.Windows.Controls.DataGridColumn   Header$ اسم المادة��6)      Width5  /System.Windows.Controls.DataGridLengthConverter$ 2* 6a      
IsReadOnly$
 True��6l   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6=   �� *RawMaterialName+6l     5(      $ الكمية��6)   $ * 6{    69   �� *
Quantity+ �� UpdateSourceTrigger$ PropertyChanged=�6{     5)      $ نسبة الهالك (%)��6)   $ * 6�    6B   �� *ScrapPercentage+$ PropertyChanged=�6�   4  .System.Windows.Controls.DataGridTemplateColumn  5*      $
 Auto 6-      CellTemplate 5+      �� 5,      .R�5-   "   �� -   5        $��حذف��5-   )   
������ 
���  ��$��0q�6z   5.      50      52      53      �� 55   
   $��2��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   $��10q�6W   .+�56      �� -   5        $ 
SaveButton��56      $��حفظ��6)   $	��100��67   $��5q�6C    �� 	IsDefault$
 True��6N   6_   �� 57      $��
إلغاء��6   $	��100��6%   $��5q�61    �� IsCancel$
 True��6<   58      59      5:      