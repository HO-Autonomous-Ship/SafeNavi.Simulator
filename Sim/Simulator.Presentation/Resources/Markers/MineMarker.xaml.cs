namespace SyDLab.Usv.Simulator.Presentation.Resources.Markers
{
	/// <summary>
	///     MineMarker.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MineMarker
	{
		public MineMarker(object context)
		{
			DataContext = context;

			InitializeComponent();

			//RotateTransform.Angle = _angleInDegree;
		}
	}
}