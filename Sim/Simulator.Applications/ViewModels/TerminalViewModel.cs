using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using SyDLab.Usv.Simulator.Applications.Views;
using SyDLab.Usv.Simulator.Domain;
using SyDLab.Usv.Simulator.Domain.Logs;

namespace SyDLab.Usv.Simulator.Applications.ViewModels
{
	[Export]
	public class TerminalViewModel : ViewModel<ITerminalView>
	{
		private DelegateCommand _commandExecute;

		[ImportingConstructor]
		public TerminalViewModel(ITerminalView view) : base(view)
		{
		}
		
		public IEnumerable<string> Items => Singleton<LogManager>.UniqueInstance.Messages;

		public DelegateCommand CommandExecute
		{
			get { return _commandExecute; }
			set { SetProperty(ref _commandExecute, value); }
		}
	}
}