   M S B A M L   `   `   `  ����  GoodMorningFactory;    5GoodMorningFactory.UI.Views.AddManualAttendanceWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	     � Title$/  )تسجيل حضور/انصراف يدوي��5      $	��300��69   $	��450��6F    � WindowStartupLocation$ CenterOwner=�5	   	   $��RightToLeft=�6-   .��5
      � $��15q�6    � ColumnDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6    � RowDefinitions 5   
   �� 5      �� 5      �� 5      �� 5      .+�5   
   �� $��0��6   $��0��6   $��الموظف:��6-   �� 5   
   -   5         ��Name$ EmployeeComboBox��5      $��0��6-   $��1��6:   $��5q�6J    �� DisplayMemberPath$ 	FirstName��6U   �� 5   
   $��1��6   $��0��6   $��التاريخ:��6-   (  "System.Windows.Controls.DatePicker  5   
   -   5          Name$ AttendanceDatePicker��5      $��1��63   $��1��6@   $��5q�6P   �� 5   
   $��2��6   $��0��6   $��الوقت (HH:mm):��6-   �� 5   
   -   5         ��Name$ TimeTextBox��5      $��2��6'   $��1��64   $��5q�6D   �� 5   
   $��3��6   $��1��6"    �� Orientation$ 
Horizontal=�62   $��5q�6K   .+�5       �� -   5        $��✅ تسجيل حضور��5       $	��120��6,   $��5q�68   $��5q�6C   6O   �� 5!      -   5        $!��❌ تسجيل انصراف��5!      $	��120��6.   $��5q�6:   $��5q�6E   6Q   5"      5#      5$      