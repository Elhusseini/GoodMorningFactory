   M S B A M L   `   `   `  ����  GoodMorningFactory2    ,GoodMorningFactory.UI.Views.GoodsReceiptView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ���#F5F5F5$��RightToLeft=�5
   #   .��5      � $��10q�6     � RowDefinitions  5   
   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   &   System.Windows.Controls.DataGrid  -   5          Name$ GrnDataGrid��5      $��0��6(   $��5q�65      
IsReadOnly$
 True��6@      AutoGenerateColumns$ False��6R      CanUserAddRows$ False��6n      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$ رقم المذكرة��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 6X   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6:   �� *	GRNNumber+6X     5      $ أمر الشراء��6%   $ * 6o    69   �� *#!PurchaseOrder.PurchaseOrderNumber+6o     5      $ المورد��6%   $ 2* 6e    65   �� *PurchaseOrder.Supplier.Name+6e     5      $! تاريخ الاستلام��6%   $ * 6o    6=   �� *ReceiptDate+	 �� StringFormat$	 d��6o   4  .System.Windows.Controls.DataGridTemplateColumn  5      $ إجراءات��6)   $
 Auto 6:   
   CellTemplate
 5      �� 5      .R�5   	   ��  �� Orientation$ 
Horizontal=�6   .+�5      �� -   5        $��عرض التفاصيل��5   %   $��2q�6<   6G   �� 5   
   -   5        $��
طباعة��5      $��2q�6!   
�������6F   5   
   5       5"      5$      �� 5&   
   $��1��6   $��0,1,0,0q�6   
������� �� Padding$ 5q�6P   $��0,5,0,0q�6\   ._�5'      �  � ColumnDefinitions 5(      �� 5)      $
��Auto �6'   �� 5*      $��* �6'   �� 5+      $
��Auto �6'   .+�5-      �� $��0��6   $ 
Horizontal=�6-   .+�5.      �� -   5         ��Name$ PreviousPageButton��5.      $��< السابق��69   $	��5,0q�6O   6\   �� 5/      -   5        $ NextPageButton��5/      $��التالي >��65   $	��5,0q�6K   6X   50      �� 51      -   5         ��Name$ PageInfoTextBlock��51      $��2��67   $��Center=�6G   $��Right=�6b   52      53      54      55      