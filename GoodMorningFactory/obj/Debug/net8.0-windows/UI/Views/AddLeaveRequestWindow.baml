   M S B A M L   `   `   `  ����  GoodMorningFactory7    1GoodMorningFactory.UI.Views.AddLeaveRequestWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	     � Title$"  تقديم طلب إجازة��5      $	��400��62   $	��500��6?    � WindowStartupLocation$ CenterOwner=�5	   	   $��RightToLeft=�6-   .��5
      � $��15q�6    � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6    � RowDefinitions 5   
   �� 5      �� 5      �� 5      �� 5      �� 5      .+�5   
   �� $��0��6   $��0��6   $��الموظف:��6-   �� 5   
   -   5         ��Name$ EmployeeComboBox��5      $��0��6-   $��1��6:   $��5q�6J    �� DisplayMemberPath$ 	FirstName��6U   �� 5   
   $��1��6   $��0��6   $��نوع الإجازة:��6-   �� 5   
   -   5        $ LeaveTypeComboBox��5      $��1��6.   $��1��6;   $��5q�6K   $
 Name��6V    �� SelectedValuePath$ Id��6o   �� 5   
   $��2��6   $��0��6   $��تاريخ البدء:��6-   (  "System.Windows.Controls.DatePicker  5   
   -   5          Name$ StartDatePicker��5      $��2��6.   $��1��6;   $��5q�6K   �� 5    
   $��3��6   $��0��6   $"��تاريخ الانتهاء:��6-     5!   
   -   5        $ EndDatePicker��5!      $��3��6,   $��1��69   $��5q�6I   �� 5#   
   -   5        $��4��5#      $��1��6   $ ��💾 تقديم الطلب��6.   $
��Left=�6G   $
��15,5q�6b   $��5q�6q   6|   5$      5%      