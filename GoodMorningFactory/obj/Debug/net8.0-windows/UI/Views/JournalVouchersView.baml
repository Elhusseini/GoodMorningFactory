   M S B A M L   `   `   `  ����  GoodMorningFactory5    /GoodMorningFactory.UI.Views.JournalVouchersView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ���#F5F5F5$��RightToLeft=�5	   #   .��5
      � $��10q�6     � RowDefinitions  5      �� 5      $
��Auto �6   �� 5      $��* �6   .+�5      �� -   5        $-��'➕ إضافة قيد يومي جديد��5      $��0��6+   $
��Left=�68   $��5q�6S   $
��10,5q�6^   6m   &   System.Windows.Controls.DataGrid  5      -   5          Name$ VouchersDataGrid��5      $��1��6'   $��5q�64      
IsReadOnly$
 True��6?      AutoGenerateColumns$ False��6Q      CanUserAddRows$ False��6m      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$ رقم القيد��6      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 6N   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6,   �� *VoucherNumber+6N     5      $ تاريخ القيد��6   $ * 6`    6.   �� *VoucherDate+	 �� StringFormat$	 d��6`     5      $ 
الوصف��6   $ 3* 6H    6(   �� *Description+6H     5      $ إجمالي المدين��6   $ * 6_    60   �� *
TotalDebit+$	 C��6_     5      $ إجمالي الدائن��6   $ * 6`    60   �� *TotalCredit+$	 C��6`     5      $ الحالة��6   $ * 6D   
   
FontWeight$

 Bold"�6N    6)   �� *Status+6N   4  .System.Windows.Controls.DataGridTemplateColumn  5      $ إجراءات��6   $
 Auto 6.      CellTemplate 5      �� 5      .R�5   	   ��  �� Orientation$ 
Horizontal=�6   .+�5   
   �� -   5        $��عكس القيد��5      $��2q�6%   
���\\��
���������5       �� 5!       �� 
TargetType" ��6   .�5"      ��  �� Property � y� �� Value$ 	Collapsed=�6*    �� Triggers 5#      �� 5$       �� Binding 6   �� *Status+ �� Value
 Posted65   5&      5(      5)   
   5*   	   5,      5/      50      51      