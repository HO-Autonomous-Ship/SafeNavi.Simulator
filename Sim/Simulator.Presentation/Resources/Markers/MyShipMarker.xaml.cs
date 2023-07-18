using System.Windows.Controls;
using SyDLab.Usv.Simulator.Applications.Views;

namespace SyDLab.Usv.Simulator.Presentation.Resources.Markers
{
	/// <summary>
	///     MyShipMarker.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MyShipMarker : UserControl
	{
		public MyShipMarker(object dataContext)
		{
			DataContext = dataContext;

			InitializeComponent();
		}
	}
}