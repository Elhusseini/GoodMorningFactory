   M S B A M L   `   `   `  ����  GoodMorningFactory9    3GoodMorningFactory.UI.Views.CustomerStatementWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     � Title$"  كشف حساب العميل��5	   	   $	��600��6!   $	��800��6.    � WindowStartupLocation$ CenterOwner=�5
   	   $��RightToLeft=�6-   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� -   5         ��Name$ CustomerNameTextBlock��5      $��0��63   $��18(�6@   $
��Bold"�6N   $��5q�6`   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ StatementDataGrid��5      $��1��6.   $��0,10,0,0q�6;      
IsReadOnly$
 True��6M      AutoGenerateColumns$ False��6_      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$ التاريخ��6%   	   Width5  /System.Windows.Controls.DataGridLengthConverter$	 * 6a   1  +System.Windows.Controls.DataGridBoundColumn
   Binding
 66   �� *Date+ �� StringFormat$ d��6a     5      $ نوع الحركة��6%   $	 * 6]   
 69   �� *TransactionType+6]     5      $ المرجع��6%   $	 * 6Y   
 65   �� *ReferenceNumber+6Y     5      $ مدين��6%   $	 * 6]   
 63   �� *Debit+$ C��6]     5      $ دائن��6%   $	 * 6^   
 63   �� *Credit+$ C��6^     5      $ الرصيد��6%   $	 * 6a      
FontWeight$
 Bold"�6k   
 65   �� *	Balance+$ C��6k   5      5      5      