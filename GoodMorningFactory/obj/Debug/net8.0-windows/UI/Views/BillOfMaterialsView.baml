   M S B A M L   `   `   `  ����  GoodMorningFactory5    /GoodMorningFactory.UI.Views.BillOfMaterialsView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ���#F5F5F5$��RightToLeft=�5
   #   .��5      � $��10q�6     � RowDefinitions  5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� -   5        $7��1➕ إنشاء قائمة مكونات جديدة��5      $��0��66   $
��Left=�6C   $��5q�6^   $
��10,5q�6i   6x   &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ BomDataGrid��5      $��1��6(   $��5q�65      
IsReadOnly$
 True��6@      AutoGenerateColumns$ False��6R      CanUserAddRows$ False��6n      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$! المنتج النهائي��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ 2* 6c   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6=   �� *FinishedGood.Name+6c     5      $ 
الوصف��6%   $ 3* 6T    64   �� *Description+6T   4  .System.Windows.Controls.DataGridTemplateColumn  5      $ إجراءات��6)   $
 Auto 6:   	   CellTemplate	 5      �� 5      .R�5      �� 
 �� Orientation$
 
Horizontal=�6)   .+�5   "   �� -   5        $��
تعديل��5   )   $��2q�69   6D   �� 5   "   -   5        $��نسخ��5   )   $��2q�67   
����·�6[   �� 5   "   -   5        $��
طباعة��5   )   $��2q�69   
�������6^   �� 5    "   -   5        $��حذف��5    )   $��2q�67   
�������6]   5!      5"      5$      5&      5'      5(      