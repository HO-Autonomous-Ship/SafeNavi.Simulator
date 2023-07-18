using System.Windows.Controls;

namespace SyDLab.Usv.Simulator.Presentation.Resources.Markers
{
	/// <summary>
	/// DetectedMineMarker.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class DetectedMineMarker : UserControl
	{
		public DetectedMineMarker(object context)
		{
			DataContext = context;

			InitializeComponent();
		}
	}
}