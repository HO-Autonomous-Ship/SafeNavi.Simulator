using System.ComponentModel.Composition;
using System.Windows.Controls;
using SyDLab.Usv.Simulator.Applications.Views;

namespace SyDLab.Usv.Simulator.Presentation.Views
{
	[Export(typeof(ITerminalView))]
	public partial class TerminalView : UserControl, ITerminalView
	{
		public TerminalView()
		{
			InitializeComponent();
		}
	}
}