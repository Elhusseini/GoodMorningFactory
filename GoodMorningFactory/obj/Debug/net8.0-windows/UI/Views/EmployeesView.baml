   M S B A M L   `   `   `  ����  GoodMorningFactory/    )GoodMorningFactory.UI.Views.EmployeesView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ���#F5F5F5$��RightToLeft=�5	   #   .��5
      � $��10q�6     � RowDefinitions  5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� -   5        $&�� ➕ إضافة موظف جديد��5      $��0��6-   $
��Left=�6:   $��5q�6U   $
��10,5q�6`   6o   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ EmployeesDataGrid��5      $��1��6.   $��5q�6;      
IsReadOnly$
 True��6F      AutoGenerateColumns$ False��6X      CanUserAddRows$ False��6t      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$ كود الموظف��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 6Z   1  +System.Windows.Controls.DataGridBoundColumn   Binding 69   �� *EmployeeCode+6Z     5      $ اسم الموظف��6%   $ 2* 6V    69   �� *
FullName+6V     5      $! المسمى الوظيفي��6%   $ * 6Z    6=   �� *
JobTitle+6Z     5      $ 
القسم��6%   $ * 6S    64   �� *
Department+6S     5      $ الحالة��6%   $ * 6P   	   
FontWeight$
	 Bold"�6Z    65   �� *Status+6Z   4  .System.Windows.Controls.DataGridTemplateColumn  5      $ إجراءات��6)   $
 Auto 6:   
   CellTemplate
 5      �� 5      .R�5      �� -   5        $��عرض / تعديل��5   %   $��2q�6;   6F   5      5      5!      5"      5#      