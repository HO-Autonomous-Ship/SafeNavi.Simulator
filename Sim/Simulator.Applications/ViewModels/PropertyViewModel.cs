using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using SyDLab.Usv.Simulator.Applications.Views;

namespace SyDLab.Usv.Simulator.Applications.ViewModels
{
	[Export]
	public class PropertyViewModel : ViewModel<IPropertyView>
	{
		private object _lastSelectedObject;
		private ObservableCollection<object> _selectedObjects;

		[ImportingConstructor]
		public PropertyViewModel(IPropertyView view) : base(view)
		{
		}

		public object LastSelectedObject
		{
			get { return _lastSelectedObject; }
			set { SetProperty(ref _lastSelectedObject, value); }
		}

		public ObservableCollection<object> SelectedObjects
		{
			get { return _selectedObjects; }
			set { SetProperty(ref _selectedObjects, value); }
		}
	}
}