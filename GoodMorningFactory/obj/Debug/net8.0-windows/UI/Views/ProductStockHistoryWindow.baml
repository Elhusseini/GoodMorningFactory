   M S B A M L   `   `   `  ����  GoodMorningFactory;    5GoodMorningFactory.UI.Views.ProductStockHistoryWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	     � Title$$  سجل حركات المنتج��5      $	��500��63   $	��800��6@    � WindowStartupLocation$ CenterOwner=�5	   	   $��RightToLeft=�6-   .��5
      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� -   5         ��Name$ ProductNameTextBlock��5      $��0��62   $��18(�6?   $
��Bold"�6M   $��5q�6_   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ HistoryDataGrid��5      $��1��6,   $��0,10,0,0q�69      
IsReadOnly$
 True��6K      AutoGenerateColumns$ False��6]      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$ التاريخ��6%   	   Width5  /System.Windows.Controls.DataGridLengthConverter$	 * 6a   1  +System.Windows.Controls.DataGridBoundColumn
   Binding
 66   �� *Date+ �� StringFormat$ g��6a     5      $ نوع الحركة��6%   $	 * 6]   
 69   �� *TransactionType+6]     5      $ رقم المرجع��6%   $	 * 6]   
 69   �� *ReferenceNumber+6]     5      $ كمية داخلة��6%   $	 * 6X   
 69   �� *
QuantityIn+   	CellStyle 5      �� 5       �� 
TargetType*  $System.Windows.Controls.DataGridCell"  6    .�5      ��  �� Property � �� �� Value �#E8F5E96;   5      5        5      $ كمية خارجة��6%   $	 * 6Y   
 69   �� *QuantityOut+ 5      �� 5       "  6    .�5!      ��  � �� �#FFEBEE6;   5"      5$      5&      5'      5(      