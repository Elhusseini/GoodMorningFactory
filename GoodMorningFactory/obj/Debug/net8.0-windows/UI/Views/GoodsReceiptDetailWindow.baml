   M S B A M L   `   `   `  ����  GoodMorningFactory:    4GoodMorningFactory.UI.Views.GoodsReceiptDetailWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	     � Title$*  $تفاصيل استلام بضاعة��5      $	��500��66   $	��700��6C    � WindowStartupLocation$ CenterOwner=�5	   	   $��RightToLeft=�6-   .��5
      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $!��تفاصيل المذكرة��6   $��0��6+   .��5      �  � ColumnDefinitions 5      �� 5      $
��Auto �6'   �� 5      $��* �6'   �� 5      $
��Auto �6'   �� 5      $��* �6'   .+�5      �� $��0��6   $��رقم المذكرة:��6(   �� 5      -   5         ��Name$ GrnNumberTextBlock��5      $��1��68   $
��Bold"�6H   $��Center=�6Z   �� 5      $��2��6   $��أمر الشراء:��6(   �� 5      -   5        $ PoNumberTextBlock��5      $��3��67   $
��Bold"�6G   $��Center=�6Y   5      5      &   System.Windows.Controls.DataGrid  5    
   -   5          Name$ ItemsDataGrid��5       $��1��6*   $��0,10,0,0q�67      
IsReadOnly$
 True��6I      AutoGenerateColumns$ False��6[      Columns 5!      0  *System.Windows.Controls.DataGridTextColumn  5"      ,  &System.Windows.Controls.DataGridColumn	   Header$	 المنتج��6%   
   Width5  /System.Windows.Controls.DataGridLengthConverter$
 2* 6V   1  +System.Windows.Controls.DataGridBoundColumn   Binding 65   �� *Product.Name+6V     5#      $#	 الكمية المستلمة��6%   $
 * 6c    6>   �� *QuantityReceived+6c   5%      �� 5'   
   -   5         ��Name$ CreateInvoiceButton��5'      $&�� إنشاء فاتورة مورد��6.   $��2��6J   $
��Left=�6W   $
��15,5q�6r   $��5q�6�   
������
�������6�   5(      5)      