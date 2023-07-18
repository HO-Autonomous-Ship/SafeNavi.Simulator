using System.ComponentModel.Composition;
using System.Waf.Applications;
using System.Windows.Input;
using SyDLab.Usv.Simulator.Applications.Views;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;

namespace SyDLab.Usv.Simulator.Applications.ViewModels
{
	[Export]
	public class PlatformEditorViewModel : ViewModel<IPlatformEditorView>
	{
		private readonly DelegateCommand _commandOk;
		private bool _dialogResult;
		private double _latitude;
		private double _longitude;
		private TargetShip _platform;

		[ImportingConstructor]
		public PlatformEditorViewModel(IPlatformEditorView view) : base(view)
		{
			_commandOk = new DelegateCommand(OkHandler);
		}

		public ICommand CommandOk => _commandOk;

		public double Latitude
		{
			get => _latitude;
			set => SetProperty(ref _latitude, value);
		}

		public double Longitude
		{
			get => _longitude;
			set => SetProperty(ref _longitude, value);
		}

		public TargetShip Platform
		{
			get => _platform;
			set => SetProperty(ref _platform, value);
		}

		public bool ShowDialog(object owner)
		{
			ViewCore.ShowDialog(owner);
			return _dialogResult;
		}

		private void OkHandler()
		{
			_dialogResult = true;
			//if (IsWasReturned) { SelectedPerson = null; }
			ViewCore.Close();
		}
	}
}