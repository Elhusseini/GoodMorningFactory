   M S B A M L   `   `   `  ����M  IGoodMorningFactory, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:GoodMorningFactory.UI.ViewsGoodMorningFactory.UI.Views  5   	   S.clr-namespace:GoodMorningFactory.UI.Converters GoodMorningFactory.UI.Converters  5   	   6    0GoodMorningFactory.UI.Views.AddEditAccountWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ;local)clr-namespace:GoodMorningFactory.UI.Views     5   	   E
converters.clr-namespace:GoodMorningFactory.UI.Converters     5   	     � Title$$  إضافة/تعديل حساب��5	   	   $	��450��6"   $	��400��6/    � WindowStartupLocation$ CenterScreen=�5
   	   $��RightToLeft=�6.    � 	Resources 5      A   ;GoodMorningFactory.UI.Converters.EnumToDescriptionConverter   EnumToDescriptionConverter��%   &	          5   
   60   5      .��5      � $��15q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� $��0��6   $��0��6   $��رقم الحساب:��6-   �� 5   
   -   5         ��Name$ AccountNumberTextBox��5   /   $��0��6   $��1��6   $��5q�6M   �� 5   
   $��1��6   $��0��6   $��اسم الحساب:��6-   �� 5    
   -   5        $ AccountNameTextBox��5    /   $��1��6   $��1��6   $��5q�6K   �� 5"   
   $��2��6   $��0��6   $��نوع الحساب:��6-   �� 5#   
   -   5         ��Name$ AccountTypeComboBox��5#   0   $��2��6   $��1��6    $��5q�6M   ��5$      �� 5%      .R�5&      �� ��6    ��  �� 	Converter# [  5'      5)      �� 5+   
   $��3��6   $��0��6   $��حساب أب:��6-   �� 5,   
   -   5        $ ParentAccountComboBox��5,   0   $��3��6   $��1��6    $��5q�6O    �� DisplayMemberPath$ AccountName��6Z   	 �� SelectedValuePath$	 Id��6z   �� 5.   
   $��5��6   $��2��6"   
 �� Orientation$
 
Horizontal=�66   $��Right=�6O   .+�5/      �� -   5        $��حفظ��5/      $	��100��6#   $��5q�6/   6:   �� 50      $��
إلغاء��6   $	��100��6%   $��5q�61    �� IsCancel$
 True��6<   51      52      53      