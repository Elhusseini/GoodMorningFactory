   M S B A M L   `   `   `  ����  GoodMorningFactory;    5GoodMorningFactory.UI.Views.AddEditJournalVoucherView   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	     � Title$)  #إضافة قيد يومي جديد��5	   	   $	��600��6%   $	��900��62    � WindowStartupLocation$ CenterOwner=�5
   	   $��RightToLeft=�6-   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��معلومات القيد��6   $��0��6*   .��5      �  � ColumnDefinitions 5      �� 5      $
��Auto �6'   �� 5      $��* �6'   �� 5      $
��Auto �6'   �� 5      $��* �6'    5      �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   .+�5       �� $��0��6   $��0��6%   $��رقم القيد:��65   �� 5!      -   5         ��Name$ VoucherNumberTextBox��5!      $��0��68   $��1��6E   $��5q�6U    �� 
IsReadOnly$
 True��6`   ���#EEE6r   �� 5#      $��0��6   $��2��6%   $��التاريخ:��65   (  "System.Windows.Controls.DatePicker  5$      -   5          Name$ VoucherDatePicker��5$      $��0��68   $��3��6E   $��5q�6U   �� 5&      $��1��6   $��0��6%   $��الوصف:��65   �� 5'      -   5        $ DescriptionTextBox��5'      $��1��66   $��1��6C   $��3��6S   $��5q�6g   5(      5*      &   System.Windows.Controls.DataGrid  5,   
   -   5          Name$ VoucherItemsDataGrid��5,      $��1��61   $
��0,10q�6>      AutoGenerateColumns$ False��6L   	   CanUserAddRows$
	 True��6h   
   Columns
 5-      4  .System.Windows.Controls.DataGridComboBoxColumn  5.      -   5        AccountColumn��,  &System.Windows.Controls.DataGridColumn   Header$ الحساب��5.   @      SelectedValuePath$ Id��6{      DisplayMemberPath$ AccountName��6�      Width5  /System.Windows.Controls.DataGridLengthConverter$ 3* 6�      SelectedValueBinding 6P   �� *	AccountId+6�   0  *System.Windows.Controls.DataGridTextColumn  5/      $ مدين��6%   $ * 6^   1  +System.Windows.Controls.DataGridBoundColumn   Binding 63   �� *Debit+ �� StringFormat$ N2��6^     50      $ دائن��6%   $ * 6_    63   �� *Credit+$ N2��6_     51      $ 
الوصف��6%   $ 2* 6T    64   �� *Description+6T     52      -   5        CostCenterColumn��$ مركز التكلفة��52   C   $
 Name��6Y   $ Id��6r   $ * 6�    6�   �� *CostCenterId+6�   55      �� 57   
   $��2��6    �� Orientation$ 
Horizontal=�6"   $��Right=�6;   .+�58      �� $%��الإجمالي المدين: ��6   $
��Bold"�61   �� 59      -   5         ��Name$ TotalDebitTextBlock��59      $
��0.00��65   $
��Bold"�6A   �� 5:      $*��$  |  الإجمالي الدائن: ��6   $
��Bold"�66   $
��10,0q�6H   �� 5;      -   5        $ TotalCreditTextBlock��5;      $
��0.00��66   $
��Bold"�6B   �� 5<      -	   5        $��💾 حفظ القيد��5<      $��20,0,0,0q�6,   $
��10,5q�6>   6M   5=      5>      5?      