   M S B A M L   `   `   `  ����  GoodMorningFactory9    3GoodMorningFactory.UI.Views.AddPurchaseReturnWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     � Title$*  $إنشاء مرتجع مشتريات��5      $	��600��66   $	��900��6C    � WindowStartupLocation$ CenterOwner=�5	   	   $��RightToLeft=�6-   .��5
      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $#��معلومات المرتجع��6   $��0��6,   .��5      �  � ColumnDefinitions 5      �� 5      $
��Auto �6'   �� 5      $��* �6'   .+�5      �� $��0��6   $7��1مرتجع من فاتورة المورد رقم:��6(   �� 5      -   5         ��Name$ InvoiceNumberTextBlock��5      $��1��6<   $
��Bold"�6L   $��Center=�6^   5      5      �� 5   
   $#��البنود المرتجعة��6   $��1��6,   $��0,10,0,0q�69   .��5      &   System.Windows.Controls.DataGrid  -   5          Name$ ReturnItemsDataGrid��5         CanUserAddRows$ False��64      AutoGenerateColumns$ False��6K      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn	   Header$	 المنتج��6)   
   
IsReadOnly$

 True��6Y      Width5  /System.Windows.Controls.DataGridLengthConverter$ 2* 6k   1  +System.Windows.Controls.DataGridBoundColumn   Binding 69   �� *ProductName+6k     5       $(	 "الكمية في الفاتورة��6)   $

 True��6j   $ * 6|    6E   �� *OriginalQuantity+6|     5!      $*	 $الكمية للإرجاع الآن��6)   $ * 6k    6F   �� *QuantityToReturn+6k   5#      5$      �� 5&   
   -   5        $#��✅ تأكيد المرتجع��5&      $��2��6+   $
��Left=�68   $��0,10,0,0q�6S   $
��15,8q�6e   6t   5'      5(      