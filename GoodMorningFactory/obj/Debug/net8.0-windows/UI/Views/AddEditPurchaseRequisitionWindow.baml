   M S B A M L   `   `   `  ����M  IGoodMorningFactory, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:GoodMorningFactory.UI.ViewsGoodMorningFactory.UI.Views  5   	   B    <GoodMorningFactory.UI.Views.AddEditPurchaseRequisitionWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ;local)clr-namespace:GoodMorningFactory.UI.Views     5   	     � Title$)  #إنشاء طلب شراء جديد��5   	   $	��600��6%   $	��800��62    � WindowStartupLocation$ CenterScreen=�5	   	   $��RightToLeft=�6.   .��5
      � $��15q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��معلومات الطلب��6   $��0��6*   $��0,0,0,10q�67   .��5      �  � ColumnDefinitions 5      �� 5      $
��Auto �6'   �� 5      $��* �6'    5      �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   .+�5      �� $��0��6   $��0��6%   $��مقدم الطلب:��65   �� 5      -   5         ��Name$ RequesterComboBox��5      $��0��66   $��1��6C   $��5q�6S    �� DisplayMemberPath$ Username��5        �� SelectedValuePath$ Username��68   ${�False��6U   �� 5"      $��1��6   $��0��6%   $��القسم:��65   �� 5$      -   5        $ DepartmentComboBox��5$      $��1��67   $��1��6D   $��5q�6T   $
 Name��5%      $
 Name��5&      �� 5)      $��2��6   $��0��6%   $#��الغرض من الشراء:��65   �� 5*      -   5         ��Name$ PurposeTextBox��5*      $��2��62   $��1��6?   $��5q�6O   5+      5,      �� 5/   
   $#��البنود المطلوبة��6   $��1��6,   .��50      &   System.Windows.Controls.DataGrid  -   5          Name$ ItemsDataGrid��50      	   AutoGenerateColumns$	 False��6.   
   CanUserAddRows$

 True��6J      Columns 51      4  .System.Windows.Controls.DataGridComboBoxColumn  52      -   5        ProductColumn��,  &System.Windows.Controls.DataGridColumn   Header$* $المنتج/المادة الخام��52   D      Width5  /System.Windows.Controls.DataGridLengthConverter$ 2* 6a      DisplayMemberPath$
 Name��54   -      SelectedValuePath$ Id��55   -      SelectedValueBinding 53   -   �� *	ProductId+55   -   0  *System.Windows.Controls.DataGridTextColumn  56      $ وصف إضافي��6)   $ 3* 6\   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6<   �� *Description+6\     57      $ الكمية��6)   $ * 6V    69   �� *
Quantity+6V     58      $ وحدة القياس��6)   $ * 6`    6>   �� *UnitOfMeasure+6`   5:      5;      �� 5>   
   $��2��6    �� Orientation$ 
Horizontal=�6"   $
��Left=�6;   $��0,10,0,0q�6V   .+�5?      �� -   5         ��Name$ 
SaveButton��5?      $��حفظ الطلب��6)   6=   �� 5@      $��
إلغاء��6    �� IsCancel$
 True��6%   $��10,0,0,0q�65   5A      5B      5C      