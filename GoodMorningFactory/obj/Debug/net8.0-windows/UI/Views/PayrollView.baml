   M S B A M L   `   `   `  ����  GoodMorningFactory-    'GoodMorningFactory.UI.Views.PayrollView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ���#F5F5F5$��RightToLeft=�5	   #   .��5
      � $��10q�6     � RowDefinitions  5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� -   5        $1��+➕ إنشاء مسير رواتب جديد��5      $��0��63   $
��Left=�6@   $��5q�6[   $
��10,5q�6f   6u   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ PayrollsDataGrid��5      $��1��6-   $��5q�6:      
IsReadOnly$
 True��6E      AutoGenerateColumns$ False��6W      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$ 
الشهر��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 6N   1  +System.Windows.Controls.DataGridBoundColumn   Binding 64   �� *Month+6N     5      $ 
السنة��6%   $ * 6M    64   �� *Year+6M     5      $! تاريخ المعالجة��6%   $ * 6q    6=   �� *DateProcessed+ �� StringFormat$ d��6q     5      $! إجمالي الرواتب��6%   $ 2* 6m   	   
FontWeight$
	 Bold"�6x    6=   �� *TotalAmount+$ C��6x   5      5      5      