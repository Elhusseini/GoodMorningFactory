   M S B A M L   `   `   `  ����M  IGoodMorningFactory, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:GoodMorningFactory.UI.ViewsGoodMorningFactory.UI.Views  5      0    *GoodMorningFactory.UI.Views.PriceListsView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ;local)clr-namespace:GoodMorningFactory.UI.Views     5      $��RightToLeft=�5	      
�������.��5
      � $��15q�6     � RowDefinitions  5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� $��0��6    �� Orientation$ 
Horizontal=�6"   $��0,0,0,15q�6;   .+�5      �� $*��$إدارة قوائم الأسعار��6   $��24(�63   $
��Bold"�6A   $��Center=�6S   �� 5      -   5         ��Name$ AddPriceListButton��5      $3��-+ إضافة قائمة أسعار جديدة��61   $��20,0,0,0q�6U   6g   5      &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ PriceListsDataGrid��5       $��1��6      AutoGenerateColumns$ False��6<      
IsReadOnly$
 True��6X      CanUserAddRows$ False��6j      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$ اسم القائمة��6%   	   Width5  /System.Windows.Controls.DataGridLengthConverter$	 2* 6S   1  +System.Windows.Controls.DataGridBoundColumn
   Binding
 6:   �� *Name+6S     5      $ 
الوصف��6%   $	 3* 6T   
 64   �� *Description+6T   4  .System.Windows.Controls.DataGridTemplateColumn  5      $ إجراءات��6)   $
	 Auto 6:      CellTemplate 5      �� 5      .R�5      �� $ 
Horizontal=�6)   .+�5   "   �� -   5        $��إدارة الأسعار��5   )   $��2q�6A   
���"�"�
�������6�   �� 5    "   -   5        $��
تعديل��5    )   $��2q�69   6D   �� 5!   "   -   5        $��حذف��5!   )   $��2q�67   
���\\��
�������6t   5"      5#      5%      5'      5(      5)      