   M S B A M L   `   `   `  ����M  IGoodMorningFactory, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:GoodMorningFactory.UI.ViewsGoodMorningFactory.UI.Views  5      :    4GoodMorningFactory.UI.Views.ProductionSchedulingView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ;local)clr-namespace:GoodMorningFactory.UI.Views     5      $��RightToLeft=�5	      
�������.��5
      � $��15q�6     � RowDefinitions  5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� $��0��6   $-��'جدولة الإنتاج (Gantt Chart)��6!   $��24(�6D   $
��Bold"�6R   $��0,0,0,15q�6d   �� 5   
   $��1��6   $
��Auto=�6$   $
��Auto=�6I   .��5      �   5      �� 5      $
��Auto �6$   �� 5      $��* �6$   .+�5      �� -   5         ��Name$ TimelineHeader��5   ,   $��0��6   ��5      �� 5      .R�5      ��  �� Orientation$ 
Horizontal=�6)   5      ��5       �� 5!      .R�5"      �� 
�������$��0,0,1,1q�6=   $��50��6W   ._�5#   "   ��  �� Padding$ 5q�6S    �� TextAlignment$ Center=�6_   ��6,   ��  �� StringFormat$ dd-MMM��6_   5$      5%      5'      �� 5*      -   5        $ GanttChartItems��5*   ,   $��1��6   ��5+      �� 5,      .R�5-      � $��40��6#   $	��0,2q�6/   .+�5.   "   �� 
����d�$��1q�6F    �� CornerRadius$ 3��6Z   $
��Left=�5/   D   ��5.   k   �� *
BarColor+��5/   )   �� *
BarWidth+��6_   �� *
LeftOffset+   LeftMarginConverter �� 	Converter# [  ._�50   &   �� $��Center=�6J   $	��5,0q�6e   
�������$��CharacterEllipsis=�6�   ��60   �� *
TaskName+6�   51   #    ^� ToolTip 52   "   _� 53   &   .��54   *   �� .+�55   .   �� $
��Bold"�68   ��6J   �� *
TaskName+�� 56   .   ��68   �� *WorkOrder.WorkOrderNumber+$ أمر العمل: {0}���� 57   .   ��68   �� *	StartDate+$" تاريخ البدء: {0:d}���� 58   .   ��68   �� *	EndDate+$( "تاريخ الانتهاء: {0:d}���� 59   .   ��68   �� *
Duration+$ المدة: {0} أيام��5:   +   5;   '   5=      5>      5@      5A      5B      5C      	 � 	Resources	 5D      5   /GoodMorningFactory.UI.Views.LeftMarginConverter��%   &	          5F   
   6$   5G      5H      