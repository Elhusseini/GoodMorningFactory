   M S B A M L   `   `   `  ����  GoodMorningFactory6    0GoodMorningFactory.UI.Views.ProcessPayrollWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	     � Title$/  )معالجة مسير رواتب جديد��5	   	   $	��700��6(   $
��1000��65    � WindowStartupLocation$ CenterOwner=�5
   	   $��RightToLeft=�6-   .��5      � $��15q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6    �� Orientation$ 
Horizontal=�6"   .+�5      �� $%��معالجة رواتب شهر:��6   �� 5      -   5         ��Name$ MonthComboBox��5      $	��100��6.   �� 5      $��سنة:��6   �� 5      -   5        $ YearComboBox��5      $��80��6-   �� 5      -   5        $��🔍 احتساب��5      $
��10,0q�6)   $
��10,2q�67   6F   5      &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ PayslipsDataGrid��5      $��1��6-   $��0,10,0,0q�6:      CanUserAddRows$ False��6L      AutoGenerateColumns$ False��6c      
IsReadOnly$
 True��6   	   Columns	 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn
   Header$
 الموظف��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ 2* 6V   1  +System.Windows.Controls.DataGridBoundColumn   Binding 65   �� *EmployeeName+6V     5      $!
 الراتب الأساسي��6%   $ * 6n    6=   �� *BasicSalary+ �� StringFormat$ N2��6n     5      $!
 إجمالي البدلات��6%   $ * 6m    6=   �� *
Allowances+$ N2��6m     5      $/
 )أيام إجازة (غير مدفوعة)��6%   $ * 6{    6F   �� *UnpaidLeaveDays+$ N1��6{     5"      $ 
 أيام غياب (خصم)��6%   $ * 6o    6>   �� *AbsenceDays+$ N1��   ElementStyle 5#      �� 5$       �� 
TargetType" ��6    .�5%      ��  �� Property � �� �� Value
 �z���6;   �� 5&       �@��Padding$	 5,2q�68   �� 5'       � ��$ Stretch=�6D   �� 5(       �@��TextAlignment$ Center=�6>   5)      5+        5/      $
 الخصومات��6%   $ * 6g    67   �� *
Deductions+$ N2�� 50      �� 51      " ��6    .�52      ��  � ��
 �����6;   �� 53       �@��Padding$	 5,2q�68   �� 54       � ��$ Stretch=�6D   �� 55       �@��TextAlignment$ Center=�6>   56      58        5;      $
 صافي الراتب��6%   $ * 6i    6:   �� *	NetSalary+$ N2�� 5<      �� 5=      " ��6    .�5>      ��  � ��
 ����6;   �� 5?       � ��$
 Bold"�6;   �� 5@       �@��Padding$	 5,2q�68   �� 5A       � ��$ Stretch=�6D   �� 5B       �@��TextAlignment$ Center=�6>   5C      5E      5G      �� 5I   
   -   5        $��2��5I      $-��'💾 حفظ واعتماد المسير��6   $
��Left=�6>   $
��15,5q�6Y   $��5q�6h   6s   5J      5K      