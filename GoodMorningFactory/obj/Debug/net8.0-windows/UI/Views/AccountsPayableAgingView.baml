   M S B A M L   `   `   `  ����  GoodMorningFactory:    4GoodMorningFactory.UI.Views.AccountsPayableAgingView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ���#F5F5F5$��RightToLeft=�5	   #   .��5
      � $��10q�6     � RowDefinitions  5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� $��0��6   $5��/تقرير أعمار الذمم الدائنة��6!   $��18(�6B   $
��Bold"�6P   $��5q�6b   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ 
ApDataGrid��5      $��1��6'      
IsReadOnly$
 True��64      AutoGenerateColumns$ False��6F      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$ المورد��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ 2* 6V   1  +System.Windows.Controls.DataGridBoundColumn   Binding 65   �� *SupplierName+6V     5      $ رقم الفاتورة��6%   $ * 6]    6;   �� *InvoiceNumber+6]     5      $# تاريخ الاستحقاق��6%   $ * 6l    6>   �� *	DueDate+ �� StringFormat$ d��6l     5      $# إجمالي الفاتورة��6%   $ * 6n    6>   �� *TotalAmount+$ C��6n     5      $! الرصيد المستحق��6%   $ * 6l   	   
FontWeight$
	 Bold"�6v    6=   �� *
BalanceDue+$ C��6v     5      $ 0-30 يوم��6%   $ * 6f    67   �� *
Bucket0_30+$ C��6f     5      $ 31-60 يوم��6%   $ * 6h    68   �� *Bucket31_60+$ C��6h     5      $ 61-90 يوم��6%   $ * 6h    68   �� *Bucket61_90+$ C��6h     5      $ أكثر من 90 يوم��6%   $ * 6n    6=   �� *BucketOver90+$ C��6n   4  .System.Windows.Controls.DataGridTemplateColumn  5      $ إجراءات��6)   $
 Auto 6:   
   CellTemplate
 5      �� 5      .R�5       �� -   5        $��تسجيل دفعة��5    %   $��2q�6:   6E   5!      5#      5%      5&      5'      