   M S B A M L   `   `   `  ����  GoodMorningFactory9    3GoodMorningFactory.UI.Views.RecordSalePaymentWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	     � Title$'  !تسجيل دفعة من عميل��5      $	��350��65   $	��450��6B    � WindowStartupLocation$ CenterOwner=�5	   	    � 
ResizeMode$ NoResize=�6-   $��RightToLeft=�6C   .��5
      � $��15q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $$��تسجيل دفعة جديدة��6   $��18(�6,   $
��Bold"�6:   $��Center=�6L   $��0,0,0,15q�6i   � 5   
   $��1��6    � ColumnDefinitions 5      �� 5      $
��Auto �6#   �� 5      $��* �6#   .+�5      �� $��0��6   $��فاتورة رقم:��6(   $��5q�6;   �� 5      -   5         ��Name$ InvoiceNumberTextBlock��5      $��1��68   $��5q�6H   $
��Bold"�6S   5      � 5   
   $��2��6    5      �� 5      �� 5       .+�5"      �� $��0��6   $��Center=�6)   .+�5#      �� $��الإجمالي��6   
�������6,   �� 5$      -   5        $ TotalAmountTextBlock��5$      $��16(�6:   $��SemiBold"�6H   5%      �� 5&      $��1��6   $��Center=�6)   .+�5'      �� $!��المستحق حالياً��6   
�������62   �� 5(      -   5        $ BalanceDueTextBlock��5(      $��16(�69   $��SemiBold"�6G   
���  ��6]   5)      5*      �� 5+   
   $��3��6   $
��0,10q�6!   �� 5,   
   $��4��6   $��Center=�6"   .+�5-      �� $+��%المبلغ المستلم الآن:��6   �� 5.      -   5         ��Name$ AmountToPayTextBox��5.      $��18(�62    �� TextAlignment$ Center=�6@   $��5q�6W   5/      �� 50   
   -   5        $��5��50      $!��✅ تأكيد الدفعة��6   $��8q�67   6C   51      52      