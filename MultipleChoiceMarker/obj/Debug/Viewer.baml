   M S B A M L   `   `   `  ����T  PMahApps.Metro, Version=1.1.2.0, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4fV;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.MetroMahApps.Metro.Controls  5   	   O KMultipleChoiceMarker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:MultipleChoiceMarkerMultipleChoiceMarker 5   	   !   MultipleChoiceMarker.Viewer   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   PControls;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   4local"clr-namespace:MultipleChoiceMarker     5   	     � Title$  Mutiple Choice Marker��5	   	   $	��600��6'   $	��800��64      AccentColorBrush(   "MahApps.Metro.Controls.MetroWindow   	GlowBrush# �   6@    � 
ResizeMode$ 	CanResize=�6o    � 	Resources 5
        GridCell �� 
TargetType*  $System.Windows.Controls.DataGridCell �� BasedOn �� Property �� Value �� Triggers	 P� Property
 P� Value  animatedGrid N� EnterActions �� TargetProperty W� From	 W� To g� Duration�� 5   
   %  &	       7 &	 �     �� 5      "  6%   8   .�5      ��  � ��
 ���� 6/   �� 5       � ��$ Center=�6=    5      P� 5      	 �@ 
IsSelected
 . . �5      ��  � ��$ 0q�6<   5      5      �� 5      " �6   .�5      ��  �@J�Opacity$ 0Q�6,    5      P� 5      	 � y�$
 Visible=�64    5      �� 5      .��5   "   �� .��5   &   W� $ Opacity�66   $	 0.0X�6Z   $	 1.0X�6e   $ 0:0:0.5D�6n   5   #   5      5       5"      5#      .��5%      � .+�5&   
   �� $
��Left=�6   $��30��60   $	��Top=�6<   $	��517��6T   .+�5'      o� $	��Top=�6   $��30��6(   $��	0,0,0,291q�64   .@�5(      m� $��_File��6   $��30��6*   .@�5)      m� -   5        $��_Save Project��5)      66   m� 5*      -   5        $��_Open Project��5*      66   m� 5+      -   5        $��_Generate Report��5+      69   5,      5-      5.      �� 5/   
   -   5        $��label_projectName��5/      $��Project Name��6_   $
��Left=�6v   $��50��6�   $��	10,30,0,0q�6�   $	��Top=�6�   $	��562��6�   $��29.333(�6�   ���	#FF40AFDE6�   � 50   
   $��0,65,0,0q�6    � ColumnDefinitions 51      �� 52      $��5* �6#   �� 53      $��2* �6#   .+�55      � $��0��6   $��0,10,0,0q�6#    � RowDefinitions 56      �� 57      $��1* �6$   �� 58      $��1* �6$   .+�5:      &   System.Windows.Controls.DataGrid  -   5          Name$ dataGrid_Review��5:      $��10,45,10,10q�64      CanUserAddRows .     CanUserResizeColumns .     CanUserReorderColumns .     CanUserResizeRows .     AutoGenerateColumns .  ���	#FF636363$��2��6�   �� 5;      $��Review��6   $
��Left=�6)   $
��Auto��6D   $��10,3,0,0q�6R   $	��Top=�6d   $
��Auto��6|   $��21.333(�6�   ���	#DD6AC3F5$!��Microsoft JhengHei UI Light)�6�   �� 5<      -   5         ��Name$ button_updateResult��5<   O   $y�Hidden=�6   $��Update Result��6l   $��Right=�6�   $��25��6�   $��	0,10,10,0q�6�   $��1��6�   $	��Top=�6�   $	��124��6�     5=      -   5        $ dataGrid_Result��5=      $y�Hidden=�64    .   .   .   .   .  $��10,0,3,3q�6�   $��1��6�   5?      � 5A      $��1��6   $��0,10,0,0q�6#    5B      �� 5C      $	��150 �6$   �� 5D      �� 5E      $��50 �6$   .+�5G        -   5        $ dataGrid_AnsKey��5G      $��0��64   $��10,45,10,10q�6A    .   .   .   .     CanUserSortColumns .   .     CanUserDeleteRows .  6�   �� 5H      $��Answer Keys��6   $
��Left=�6.   $��37��6I   $��10,3,0,0q�6U   $	��Top=�6g   $	��130��6   $��21.333(�6�   ���	#DD6AC3F5$!��Microsoft JhengHei UI Light)�6�   �� 5I      -	   5         ��Name$" textBox_MarksForEachQuestion��5I      $
��Left=�6@   $��26��6[   $��	10,79,0,0q�6g   $��1��6z    �� TextWrapping$
 Wrap=�6�   $	��Top=�6�   $	��152��6�   �� 5J      $��Marks for each question��6   $
��Left=�6:   $��26��6U   $��	10,48,0,0q�6a   $��1��6t   $	��Top=�6�   $	��206��6�   $��Center=�6�   ���	#FF4F4F4F6�   �� 5K      -
   5        $% textBox_CorrectPercentageToPass��5K      $
��Left=�6C   $��26��6^   $��
10,141,0,0q�6j   $��1��6~   $
 Wrap=�6�   $	��Top=�6�   $	��152��6�   �� 5L      $ ��Correct percentage to pass��6   $
��Left=�6=   $��26��6X   $��
10,110,0,0q�6d   $��1��6x   $	��Top=�6�   $	��206��6�   $��Center=�6�   ���	#FF4F4F4F6�   �� 5M      $��Settings��6   $
��Left=�6+   $��37��6F   $��10,9,0,0q�6R   $	��Top=�6d   $	��130��6|   $��21.333(�6�   ���	#DD6AC3F5$!��Microsoft JhengHei UI Light)�6�   $��1��6�   �� 5N      $��Marks��6   $
��Left=�6(   $��26��6C   $��
167,79,0,0q�6O   $	��Top=�6c   $
��Auto��6{   $��Right=�6�   $��1��6�   �� 5O      $��%��6   $
��Left=�6$   $��26��6?   $��167,141,0,0q�6K   $	��Top=�6`   $
��Auto��6x   $��Right=�6�   $
��Bold"�6�    J� RenderTransformOrigin$ 1.765,0.712&�6�   $��1��6�   �� 5P      -   5        $ button_SaveSettings��5P      $��Save settings��6X     MetroFlatButton#��[ 6p   $
��Left=�6�   $��28��6�   $��
10,193,0,0q�6�   $��1��6�   $	��Top=�6�   $	��206��6�   5Q      5S      5T      5U      