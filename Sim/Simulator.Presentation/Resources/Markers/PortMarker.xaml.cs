using System.Windows.Controls;

namespace SyDLab.Usv.Simulator.Presentation.Resources.Markers
{
	/// <summary>
	/// PortMarker.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class PortMarker : UserControl
	{
		public PortMarker(object context)
		{
			DataContext = context;

			InitializeComponent();
		}
	}
}