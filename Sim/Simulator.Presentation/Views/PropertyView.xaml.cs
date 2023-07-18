using System.ComponentModel.Composition;
using System.Windows.Controls;
using SyDLab.Usv.Simulator.Applications.Views;

namespace SyDLab.Usv.Simulator.Presentation.Views
{
	[Export(typeof(IPropertyView))]
	public partial class PropertyView : UserControl, IPropertyView
	{
		public PropertyView()
		{
			InitializeComponent();
		}
	}
}