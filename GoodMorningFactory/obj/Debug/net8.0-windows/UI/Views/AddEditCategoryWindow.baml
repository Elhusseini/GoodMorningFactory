   M S B A M L   `   `   `  ����  GoodMorningFactory7    1GoodMorningFactory.UI.Views.AddEditCategoryWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	     � Title$$  إضافة / تعديل فئة��5      $	��250��64   $	��450��6A    � WindowStartupLocation$ CenterOwner=�5	   	   $��RightToLeft=�6-   .��5
      � $��15q�6    � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6    � RowDefinitions 5   
   �� 5      �� 5      �� 5      �� 5      .+�5   
   �� $��0��6   $��0��6   $��اسم الفئة:��6-   �� 5   
   -   5         ��Name$ NameTextBox��5      $��0��6'   $��1��64   $��5q�6D   �� 5   
   $��1��6   $��0��6   $��الوصف:��6-   �� 5   
   -   5        $ DescriptionTextBox��5      $��1��6.   $��1��6;   $��5q�6K   �� 5   
   $��2��6   $��0��6   $��الفئة الأم:��6-   �� 5   
   -   5         ��Name$ ParentCategoryComboBox��5      $��2��63   $��1��6@   $��5q�6P    �� DisplayMemberPath$
 Name��6[    �� SelectedValuePath$ Id��6t   �� 5   
   -   5        $��💾 حفظ��5      $��3��6"   $��1��6/   $
��Left=�6?   $	��100��6Z   $��5q�6f   6q   5      5      