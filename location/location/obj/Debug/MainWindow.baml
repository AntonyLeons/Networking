   M S B A M L   `   `   `  ����C  ?location, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:locationlocation  5   	       location.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   (localclr-namespace:location     5   	   -   5          �Name$  location��5   	    � Title$ location��5   	   $	��450��6   $	��770��6'   .��5	      � .+�5
   
   �� -   5         ��Name$ LblHost��5
      $
��Host��6!   $
��Left=�60   $��	22,32,0,0q�6K   $	��Top=�6^   �� 5   
   -   5         ��Name$ Address��5      $
��Left=�6#   $��23��6>   $��	70,35,0,0q�6J    �� TextWrapping$
 Wrap=�6]   $	��Top=�6q   $	��302��6�   $��whois.net.dcs.hull.ac.uk��6�   �� 5   
   -   5        $ LblPort��5      $
��Port��6!   $
��Left=�60   $��
400,35,0,0q�6K   $	��Top=�6_   $��37��6w   �� 5   
   -   5        $
 Port��5      $
��Left=�6    $��23��6;   $��
461,38,0,0q�6G   $
 Wrap=�6[   $	��Top=�6o   $	��162��6�   $��43��6�   �� 5   
   -   5        $ LblUsername��5      $��Username��6%   $
��Left=�68   $��2,76,0,0q�6S   $	��Top=�6e   �� 5   
   -   5        $
 User��5      $
��Left=�6    $��23��6;   $��	70,79,0,0q�6G   $
 Wrap=�6Z   $	��Top=�6n   $	��553��6�   �� 5   
   -   5        $ LblLocation��5      $��Location��6%   $
��Left=�68   $��	2,118,0,0q�6S   $	��Top=�6f   �� 5   
   -	   5        $ Location��5      $
��Left=�6$   $��23��6?   $��
70,121,0,0q�6K   $
 Wrap=�6_   $	��Top=�6s   $	��553��6�   �� 5   
   -
   5        $ Status��5      $
��Left=�6"   $	��203��6=   $��
70,191,0,0q�6J   $
 Wrap=�6^   $	��Top=�6r   $	��553��6�   
���   �
�������" �� VerticalScrollBarVisibility$
 Auto=�6�    �� 
IsReadOnly .  �� 
CaretBrush
 �����6  �� 5   
   -   5         ��Name$ 	btnSubmit��5      $��Submit��6$   $
��Left=�65   $��669,124,0,0q�6P   $	��Top=�6e   $��75��6}   	 �� 	IsDefault	 . 6�   	� 5   
   -   5        
 	�Name$
 Whois��5      $��Whois��6%   $
��Left=�65   $��180,158,0,0q�6P   $	��Top=�6e    d� 	IsChecked$
 TrueY�6}   	� 5   
   -   5        $
 Http0_9��5      $��Http 0.9��6'   $
��Left=�6:   $��280,158,0,0q�6U   $	��Top=�6j   	� 5   
   -   5        $
 Http1_0��5      $��Http 1.0��6'   $
��Left=�6:   $��380,158,0,0q�6U   $	��Top=�6j   	� 5   
   -   5        $
 Http1_1��5      $��Http 1.1��6'   $
��Left=�6:   $��480,158,0,0q�6U   $	��Top=�6j   �� 5   
   $��Timeout��6   $
��Left=�6"   $��669,191,0,0q�6=   $	��Top=�6R   �� 5   
   -   5        $ TimeBox��5      $
��Left=�6#   $��23��6>   $��666,222,0,0q�6J   $
 Wrap=�6_   $
��1000��6s   $	��Top=�6   $��62��6�    �� TextAlignment$ Center=�6�   �� 5   
   -   5         ��Name$ Debug��5      $��Debug��6"   $
��Left=�62   $��671,311,0,0q�6M   $	��Top=�6b   5      5      