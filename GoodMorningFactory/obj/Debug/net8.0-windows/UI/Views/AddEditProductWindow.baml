   M S B A M L   `   `   `  ����M  IGoodMorningFactory, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:GoodMorningFactory.UI.ViewsGoodMorningFactory.UI.Views  5   	   S.clr-namespace:GoodMorningFactory.UI.Converters GoodMorningFactory.UI.Converters  5   	   6    0GoodMorningFactory.UI.Views.AddEditProductWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ;local)clr-namespace:GoodMorningFactory.UI.Views     5   	   E
converters.clr-namespace:GoodMorningFactory.UI.Converters     5   	     � Title$&   إضافة / تعديل منتج��5	   	   $	��650��6$   $	��800��61    � WindowStartupLocation$ CenterScreen=�5
   	   $��RightToLeft=�5   	    � 	Resources 5      A   ;GoodMorningFactory.UI.Converters.EnumToDescriptionConverter   EnumToDescriptionConverter �� 
TargetType �� Property �� Value��%�  &	        '��      '�� �     '�� �     '�� l      5   
   60   �� 5   
   " ��6   .�5      ��  � ��$ 0,5,0,2q�6'   �� 5       � ��$ SemiBold"�6+   5      �� 5   
   " ��6   .�5      ��  � ��$ 3q�6(   �� 5       � ��$ 0,0,0,10q�6'   5      �� 5   
   " ��6   .�5      ��  � ��$ 3q�6(   �� 5       � ��$ 0,0,0,10q�6'   5      �� 5   
   " ��6   .�5      ��  � ��$ 0,5,0,10q�6'   5      5       .��5"      �  � RowDefinitions 5#   
   �� 5$      $��* �6   �� 5%      $
��Auto �6   .+�5(   
   �� $��0��6   $��10q�6"   .@�5*      �� $!��معلومات أساسية��6   .��5+      �� $
��Auto=�6   .��5,      �  � ColumnDefinitions 5-      �� 5.      $��* �6/   �� 5/      $	��200 �6/   .+�52      �� $��0��6%   $��10q�65   .+�53      �� $��كود المنتج (*):��6(   �� 54      -   5         ��Name$ ProductCodeTextBox��54   &   �� 55      $��اسم المنتج (*):��6(   �� 56      -   5        $ NameTextBox��56   &   �� 57      $��الوصف:��6(   �� 58      -   5        $ DescriptionTextBox��58   &   $��60��6B   	 �� TextWrapping$
	 Wrap=�6N   
 �� AcceptsReturn$

 True��6b   �� 59      $��نوع المنتج (*):��6(   �� 5:      -   5         ��Name$ ProductTypeComboBox��5:   '   ��5;   "   �� 5<   &   .R�5=   *   �� ��64   ��  �� 	Converter# [  5>   '   5@      �� 5A      $��فئة المنتج (*):��6(   �� 5B      -   5        $ CategoryComboBox��5B   '    �� DisplayMemberPath$
 Name��6A    �� SelectedValuePath$ Id��6Z   �� 5C      $��باركود:��6(   �� 5D      -   5        $ BarcodeTextBox��5D   &   �� 5E      -   5         ��Name$ IsActiveCheckBox��5E   '   $��المنتج نشط��6A   5F      �� 5H      $��1��6%   $��10,10,20,10q�65   .+�5I      �� 
�������$��1q�68   $	��150��6L   $	��150��6Y   $��0,0,0,10q�6e   ._�5J   "   �� -   5         ��Name$ ProductImage��5J   (   $��Uniform=�6>   5K      �� 5L      -	   5         ��Name$ UploadImageButton��5L   %   $��رفع صورة��6@   6S   5M      5N      5O      5P      �� 5S      $%��التسعير والتكلفة��6   .��5T      �� $
��Auto=�6   .��5U      �� $��10q�6!   .+�5V      �� $��سعر الشراء:��6$   �� 5W      -
   5        $ PurchasePriceTextBox��5W   "   �� 5X      $��سعر البيع:��6$   �� 5Y      -   5        $ SalePriceTextBox��5Y   "   �� 5Z      $��العملة (*):��6$   �� 5[      -   5        $ CurrencyComboBox��5[   #   $
 Name��6=   $ Id��6V   �� 5\      $ ��قائمة الأسعار:��6$   �� 5]      -   5        $ PriceListComboBox��5]   #   $
 Name��6>   $ Id��6W   �� 5^      $/��)قاعدة الضريبة المطبقة:��6$   �� 5_      -   5        $ TaxRuleComboBox��5_   #   $
 Name��6<   $ Id��6U   5`      5a      5b      �� 5e      $%��المخزون والتوريد��6   .��5f      �� $
��Auto=�6   .��5g      �� $��10q�6!   .+�5h      �� -   5        $ TrackInventoryCheckBox��5h   #   $3��-تتبع المخزون لهذا المنتج��6C   �� 5i      $��وحدة القياس:��6$   �� 5j      -   5        $ UnitOfMeasureComboBox��5j   #   $
 Name��6B   $ Id��6[   �� 5k      $!��حد إعادة الطلب:��6$   �� 5l      -   5        $ ReorderLevelTextBox��5l   "   �� 5m      $+��%الحد الأدنى للمخزون:��6$   �� 5n      -   5        $ MinStockLevelTextBox��5n   "   �� 5o      $+��%الحد الأقصى للمخزون:��6$   �� 5p      -   5        $ MaxStockLevelTextBox��5p   "   �� 5q      $&�� المورد الافتراضي:��6$   �� 5r      -   5        $ DefaultSupplierComboBox��5r   #   $
 Name��6D   $ Id��6]   �� 5s      $-��'مدة التوريد (بالأيام):��6$   �� 5t      -   5        $ LeadTimeDaysTextBox��5t   "   �� 5u      $��طريقة التتبع:��6    $
��Bold"�68   $��0,15,0,0q�6J   �� 5v      -   5        $ TrackingMethodComboBox��5v   #   $��5q�6C   $ Value��5w   #   $ Description��6=   5x      5y      5z      �� 5}      $!��نسخ من منتج آخر��6   .��5~      �� $��15q�6   .+�5      �� $p��jيمكنك هنا نسخ البيانات من منتج موجود لتسريع عملية الإدخال.��6    $
��Wrap=�6b   $��0,0,0,20q�6v   �� 5�      $?��9اختر المنتج المراد نسخ بياناته:��6    �� 5�      -   5        $ CopyFromProductComboBox��5�       �� 
IsEditable$
 True��5�       �� StaysOpenOnEdit$
 True��5�      $
 Name��5�      $ Id��5�      5�      �� 5�      -   5        $ CopyDataButton��5�      $��نسخ البيانات��65   $��0,10,0,0q�6L   $
��Left=�6^   6y   5�      5�      5�      �� 5�   
   $��1��6   ���#F0F0F0
�������$��0,1,0,0q�6K   ._�5�      ��  �� Orientation$ 
Horizontal=�6   $��Right=�62   $��10q�6N   .+�5�      �� -   5        $ 
SaveButton��5�      $��حفظ��6-   $	��100��6;   $��5q�6G    �� 	IsDefault$
 True��6R   6c   �� 5�      $��
إلغاء��6   $	��100��6)   $��5q�65    �� IsCancel$
 True��6@   5�      5�      5�      5�      