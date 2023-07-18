using System.Windows.Controls;

namespace SyDLab.Usv.Simulator.Presentation.Resources.Markers
{
	/// <summary>
	/// WaypointMarker.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class WaypointMarker : UserControl
	{
		public WaypointMarker(object waypoint)
		{
			DataContext = waypoint;

			InitializeComponent();
		}
	}
}