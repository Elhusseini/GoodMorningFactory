   M S B A M L   `   `   `  ����M  IGoodMorningFactory, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:GoodMorningFactory.UI.ViewsGoodMorningFactory.UI.Views  5      +    %GoodMorningFactory.UI.Views.RolesView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ;local)clr-namespace:GoodMorningFactory.UI.Views     5      $��RightToLeft=�5	      ���#F5F5F5  � 	Resources  5       �� 
TargetType   ActionButtonStyle �� Property �� Value��%�   &	        �� 5   
   " ��6   .�5      ��  � ��$	 8,4q�6(   �� 5       � ��$ 2q�6'   �� 5       �@�Cursor$
 Hand��6'   �� 5       � ��$ 1q�60   �� 5       � �� �#CCC6,   5      5      .��5      � $��15q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6    �� Padding$ 10q�6   $��0,0,0,10q�6+   
������� �� CornerRadius$ 5��6U   ._�5      �  � ColumnDefinitions 5       �� 5!      $
��Auto �6'   �� 5"      $��* �6'   .+�5$      �� -   5        $��0��5$      $
��10,5q�6E   $��0,0,20,0q�6T   ���#28a745
�������$��Center=�6�   .��5%      ��  �� Orientation$ 
Horizontal=�6!   .+�5&      �� $	����6$   $��Segoe MDL2 Assets)�64   $��Center=�6S   $��0,0,5,0q�6n   �� 5'      $ ��إضافة دور جديد��6$   5(      5)      �� 5*      $��1��6   $ 
Horizontal=�6-   $��Right=�6F   .+�5+      �� $��بحث:��6   $��Center=�6+   �� 5,      -   5        	 ��Name$	 SearchTextBox��5,      $	��250��65   $��Center=�6A   6\   5-      5.      5/      &   System.Windows.Controls.DataGrid  52   
   -   5        
  Name$
 RolesDataGrid��52       $��1��6      AutoGenerateColumns$ False��67      
IsReadOnly$
 True��6S      CanUserAddRows$ False��6e      Columns 53      0  *System.Windows.Controls.DataGridTextColumn  54      ,  &System.Windows.Controls.DataGridColumn   Header$ اسم الدور��6%      
FontWeight$
 Bold"�6Q      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 6c   1  +System.Windows.Controls.DataGridBoundColumn   Binding 68   �� *Name+6c     55      $ 
الوصف��6%   $ 2* 6T    64   �� *Description+6T     56      $! عدد المستخدمين��6%   $
 Auto 6[    6=   �� *	UserCount+6[   4  .System.Windows.Controls.DataGridTemplateColumn  57      $ إجراءات��6)   $
 Auto 6:      CellTemplate 58      �� 59      .R�5:      �� $ 
Horizontal=�6)   .+�5;   "   �� -   5         � ToolTip$# إدارة الصلاحيات��5;   )   #��[  6i   ���#007bff.��5<   &   �� $	����60   $��Segoe MDL2 Assets)�6@   
�������6_   5=   #   �� 5>   "   -   5        $ 
تعديل��5>   )   #��[  6R   ���#ffc107.��5?   &   �� $	����60   $��Segoe MDL2 Assets)�6@   
�������6_   5@   #   �� 5A   "   -   5        $ حذف��5A   )   #��[  6R   ���#dc3545.��5B   &   �� $	����60   $��Segoe MDL2 Assets)�6@   
�������6_   5C   #   5D      5E      5G      5I      �� 5L   
   $��2��6   $ 5q�6   $��0,10,0,0q�6*   
�������$ 5��6T   ._�5M      �  5N      �� 5O      $
��Auto �6'   �� 5P      $��* �6'   �� 5Q      $
��Auto �6'   .+�5S      �� $��0��6   $ 
Horizontal=�6-   .+�5T      �� -   5         ��Name$ PreviousPageButton��5T      $��< السابق��69   $	��5,0q�6p   �� 5U      -   5        $ NextPageButton��5U      $��التالي >��65   $	��5,0q�6h   5V      �� 5W      -	   5         ��Name$ PageInfoTextBlock��5W      $��1��67   $��Center=�6G   $��Center=�6d   5X      5Y      5Z      5[      