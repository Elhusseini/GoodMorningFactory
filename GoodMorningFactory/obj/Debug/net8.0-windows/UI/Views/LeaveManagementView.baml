   M S B A M L   `   `   `  ����  GoodMorningFactory5    /GoodMorningFactory.UI.Views.LeaveManagementView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ���#F5F5F5$��RightToLeft=�5	   #   .��5
      � $��10q�6     � RowDefinitions  5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� -   5        $/��)➕ تقديم طلب إجازة جديد��5      $��0��62   $
��Left=�6?   $��5q�6Z   $
��10,5q�6e   6t   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ LeaveRequestsDataGrid��5      $��1��62   $��5q�6?      
IsReadOnly$
 True��6J      AutoGenerateColumns$ False��6\      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$ الموظف��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ 2* 6\   1  +System.Windows.Controls.DataGridBoundColumn   Binding 65   �� *Employee.FirstName+6\     5      $ نوع الإجازة��6%   $ * 6]    6:   �� *LeaveType.Name+6]     5      $ تاريخ البدء��6%   $ * 6j    6:   �� *	StartDate+ �� StringFormat$ d��6j     5      $! تاريخ الانتهاء��6%   $ * 6k    6=   �� *	EndDate+$ d��6k     5      $ الحالة��6%   $ * 6P   	   
FontWeight$
	 Bold"�6Z    65   �� *Status+6Z   4  .System.Windows.Controls.DataGridTemplateColumn  5      $ إجراءات��6)   $
 Auto 6:   
   CellTemplate
 5      �� 5      .R�5      ��  �� Orientation$ 
Horizontal=�6)   .+�5   "   �� -   5        $��موافقة��5   )   $��2q�6:   
�������6a   �� 5   "   -   5        $��رفض��5   )   $��2q�67   
���\\��
�������6t   5      5       5"      5$      5%      5&      