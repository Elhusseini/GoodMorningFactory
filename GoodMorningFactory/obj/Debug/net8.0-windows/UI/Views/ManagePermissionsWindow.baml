   M S B A M L   `   `   `  ����M  IGoodMorningFactory, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:GoodMorningFactory.UI.ViewsGoodMorningFactory.UI.Views  5   	   S.clr-namespace:GoodMorningFactory.UI.ViewModels GoodMorningFactory.UI.ViewModels  5   	   9    3GoodMorningFactory.UI.Views.ManagePermissionsWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ;local)clr-namespace:GoodMorningFactory.UI.Views     5   	   E
viewModels.clr-namespace:GoodMorningFactory.UI.ViewModels     5   	     � Title$#  إدارة الصلاحيات��5	   	   $	��600��6!   $	��500��6.    � WindowStartupLocation$ CenterScreen=�5
   	   $��RightToLeft=�5   	   $��Segoe UI)�5   	   .��5      � $��15q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   �� -   5         ��Name$ RoleNameTextBlock��5      $��0��6/   $��18(�6<   $
��Bold"�6J   $��0,0,0,10q�6\   $!��صلاحيات الدور: ��6n   �� 5   
   $��1��6   ���#CCCCCC$��1q�64   ._�5      R� -   5         R�Name$ PermissionsTreeView��5      ��64   �� *PermissionGroups+��5      �� 5       �� DataType?   9GoodMorningFactory.UI.ViewModels.PermissionGroupViewModel"  6/    �� ItemsSource 6g   �� *Permissions+.R�5      ��  �� Orientation$ 
Horizontal=�6%   .+�5      �� $��Center=�6X    d� 	IsChecked 6'   �� *IsAllSelected+	 �� Mode$	 TwoWay=�6X   �� 5      $
��Bold"�6D   $��14(�6V   $	��5,0q�6d   ��6(   �� *
ModuleName+6d   5       
 �� ItemTemplate
 5!      �� 5"      :   4GoodMorningFactory.UI.ViewModels.PermissionViewModel"  6+   .R�5#   "   �� $ 
Horizontal=�6-   $��20,5,0,5q�6F   .+�5$   &   �� $��Center=�6]    6/   �� *
IsSelected+$	 TwoWay=�6]   �� 5%   &   $	��5,0q�6M   ��60   �� *Description+6M   5&   #   5'      5)      5+      5,      �� 5/   
   -   5         ��Name$ CopySection��5/      $��2��6&   $��0,10,0,0q�63    �� Padding$ 10q�6E   ���#F5F5F5 �� CornerRadius$ 5��6g   ._�50      �� $ 
Horizontal=�6   .+�51      �� $,��&نسخ الصلاحيات من دور:��6   $��Center=�68   $��SemiBold"�6S   �� 52      -   5         ��Name$ CopyFromRoleComboBox��52      $	��150��69   $	��5,0q�6E   �� 53      -   5        $��نسخ الآن��53      $
��10,5q�6J   $	��5,0q�6Y   ���#17A2B8
�������$��0q�6�   54      55      �� 59   
   $��3��6   $ 
Horizontal=�6"   $
��Left=�6;   $��0,15,0,0q�6V   .+�5:      �� -   5         ��Name$ 
SaveButton��5:      $��حفظ الصلاحيات��6)   $
��15,8q�6_   ���#28A745
�������$��0q�6�   �� 5;      -   5        $ CancelButton��5;      $��
إلغاء��6+    �� IsCancel$
 True��6;   $
��15,8q�6K   $��10,0,0,0q�6Z   ���#6C757D
�������$��0q�6�   5<      5=      5>      