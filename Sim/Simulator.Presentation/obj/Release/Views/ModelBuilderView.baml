   M S B A M L   `   `   `  ���� G  CUsvSimulator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullk:clr-namespace:SyDLab.Usv.Simulator.Presentation.Converters,SyDLab.Usv.Simulator.Presentation.Converters  [ WComplexDataTemplates, Version=1.0.0.0, Culture=neutral, PublicKeyToken=dda93a23bface736�Tclr-namespace:DaisleyHarrison.WPF.ComplexDataTemplates;assembly=ComplexDataTemplates(DaisleyHarrison.WPF.ComplexDataTemplates a5clr-namespace:SyDLab.Usv.Simulator.Presentation.Views'SyDLab.Usv.Simulator.Presentation.Views  V RSyDLab.Usv.Simulator.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null|Uclr-namespace:SyDLab.Usv.Simulator.Domain.Models;assembly=SyDLab.Usv.Simulator.Domain"SyDLab.Usv.Simulator.Domain.Models �_clr-namespace:SyDLab.Usv.Simulator.Domain.Models.Platforms;assembly=SyDLab.Usv.Simulator.Domain,SyDLab.Usv.Simulator.Domain.Models.Platforms N JMultiSelectTreeView, Version=1.0.9.0, Culture=neutral, PublicKeyToken=null^Bclr-namespace:System.Windows.Controls;assembly=MultiSelectTreeViewSystem.Windows.Controls �[clr-namespace:SyDLab.Usv.Simulator.Domain.Models.Tasks;assembly=SyDLab.Usv.Simulator.Domain(SyDLab.Usv.Simulator.Domain.Models.Tasks \ XSyDLab.Usv.Simulator.Applications, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null�eclr-namespace:SyDLab.Usv.Simulator.Applications.ViewModels;assembly=SyDLab.Usv.Simulator.Applications,SyDLab.Usv.Simulator.Applications.ViewModels >    8SyDLab.Usv.Simulator.Presentation.Views.ModelBuilderView   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R	 NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml    	 Hc:clr-namespace:SyDLab.Usv.Simulator.Presentation.Converters    	 hcomplexTclr-namespace:DaisleyHarrison.WPF.ComplexDataTemplates;assembly=ComplexDataTemplates    	 @d2http://schemas.microsoft.com/expression/blend/2008    	 Glocal5clr-namespace:SyDLab.Usv.Simulator.Presentation.Views    	 cmUclr-namespace:SyDLab.Usv.Simulator.Domain.Models;assembly=SyDLab.Usv.Simulator.Domain    	 Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006    	 u	platforms_clr-namespace:SyDLab.Usv.Simulator.Domain.Models.Platforms;assembly=SyDLab.Usv.Simulator.Domain    	 PtBclr-namespace:System.Windows.Controls;assembly=MultiSelectTreeView    	 mtasks[clr-namespace:SyDLab.Usv.Simulator.Domain.Models.Tasks;assembly=SyDLab.Usv.Simulator.Domain    	 |
viewModelseclr-namespace:SyDLab.Usv.Simulator.Applications.ViewModels;assembly=SyDLab.Usv.Simulator.Applications    	   � 	Resources  O  IDaisleyHarrison.WPF.ComplexDataTemplates.ComplexGroupDataTemplateSelector $   ComplexGroupDataTemplateSelectorD  >DaisleyHarrison.WPF.ComplexDataTemplates.ComplexGroupConverter  ComplexGroupConverter��%   &	        &	          .��]� .+�1  +System.Windows.Controls.MultiSelectTreeView  -     Name$ TreeView��   AllowEditItems . #��[  ���� *Nodes+   SelectedItems �� *SelectedObjects+����  �� 
TargetType5  /System.Windows.Controls.MultiSelectTreeViewItem"  .���  �� Property �@ 
IsExpanded �� Value .    �� DataType0  *SyDLab.Usv.Simulator.Domain.Models.Project �� ItemsSource	 `� 	Converter
 �� Path �� ItemContainerStyle �� Orientation �� Padding  IEnumerable[MyShip]9  3SyDLab.Usv.Simulator.Domain.Models.Platforms.MyShip  IEnumerable[TargetShip]=  7SyDLab.Usv.Simulator.Domain.Models.Platforms.TargetShip
 � Tag �� RelativeSource �� AncestorType � ContextMenu � DataContext m� Command m� Icon  IEnumerable[Obstacle]1  +SyDLab.Usv.Simulator.Domain.Models.Obstacle  IEnumerable[ScenarioBase];	  5SyDLab.Usv.Simulator.Domain.Models.ScenarioFollowPath d� 	IsChecked9
  3SyDLab.Usv.Simulator.Domain.Models.Tasks.FollowPath��%y  (��       *, +)7  &	 ^    (�� S    *, +)&	 �    (�� �    *, +)&	 �    (�� �    *, +)&	 b    (�� ^	    *,	 +)(�� D
    *,
 +)�� "   `� 8	   .1��� $
 MyShips��� $
 TargetShips��� $
 	Obstacles��� $
 	Scenarios� �� "  .���  �@ 
IsEditable .  ��  �@ 
IsExpanded . .R��� $ 
Horizontal=�.+��� $��16��$��16��$@��:/UsvSimulator;component/Resources/Images/GameMonitor16.png���� $ 4,0,2,0q����� *DisplayName+��  �� $
 .� �� "  .���  �@ 
IsEditable . ��  �@ 
IsExpanded . .R��� $ 
Horizontal=�.+��� $��16��$��16��$;��5/UsvSimulator;component/Resources/Images/Bricks16.png���� $ 4,0,2,0q�$��My Ship���� "  .R��� $ 
Horizontal=�.+��� $��16��$��16��$7��1/UsvSimulator;component/Resources/Images/helm.png���� $ 4,0,2,0q����� *DisplayName+��  �� $
 .� �� "  .���  �@ 
IsEditable . ��  �@ 
IsExpanded . .R��� $ 
Horizontal=�.+��� $��16��$��16��$;��5/UsvSimulator;component/Resources/Images/Bricks16.png���� $ 4,0,2,0q�$��Target Ships���� "  .R��� $ 
Horizontal=� �� *DataContext+ �� $ local:ModelBuilderViewL�.+��� $��16��$��16��$?��9/UsvSimulator;component/Resources/Images/Sail-Boat-48.png���� $ 4,0,2,0q����� *DisplayName+ ��  �� *PlacementTarget.Tag+ �� *Self+.@�m� $��Remove�� �� *CommandDelete+ �� $��16��$��16��$9��3/UsvSimulator;component/Resources/Images/cancel.png��m� $
��Save�� �� *CommandSaveTrack+��  �� $
 .� �� "  .���  �@ 
IsEditable . ��  �@ 
IsExpanded . .R��� $ 
Horizontal=�.+��� $��16��$��16��$;��5/UsvSimulator;component/Resources/Images/Bricks16.png���� $ 4,0,2,0q�$��	Obstacles���� "  .R��� $ 
Horizontal=� �� *DataContext+ �� $ local:ModelBuilderViewL�.+��� $��16��$��16��$<��6/UsvSimulator;component/Resources/Images/mountains.png���� $ 4,0,2,0q����� *DisplayName+ ��  �� *PlacementTarget.Tag+ �� *Self+.@�m� $��Remove�� �� *CommandDelete+ �� $��16��$��16��$9��3/UsvSimulator;component/Resources/Images/cancel.png����  �� $
 .� �� "  .���  �@ 
IsEditable . ��  �@ 
IsExpanded . .R��� $ 
Horizontal=�.+��� $��16��$��16��$@��:/UsvSimulator;component/Resources/Images/ScriptsText16.png���� $ 4,0,2,0q�$��	Scenarios���� " 	  �� *Tasks+.R��� $ 
Horizontal=�.+���  �� *
IsSelected+�� $�� ���� $��16��$��16��$?��9/UsvSimulator;component/Resources/Images/Controller16.png���� $ 4,0,2,0q����� *DisplayName+�� " 
 .R��� $ 
Horizontal=� �� *DataContext+ �� $ local:ModelBuilderViewL�.+��� $��16��$��16��$?��9/UsvSimulator;component/Resources/Images/Controller16.png���� $ 4,0,2,0q����� *DisplayName+ ��  �� *PlacementTarget.Tag+ �� *Self+.@�m� $��Remove�� �� *CommandDelete+ �� $��16��$��16��$9��3/UsvSimulator;component/Resources/Images/cancel.png��