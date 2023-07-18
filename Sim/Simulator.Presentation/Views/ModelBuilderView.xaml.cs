using SyDLab.Usv.Simulator.Applications.ViewModels;
using SyDLab.Usv.Simulator.Applications.Views;
using System;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using System.Windows;
using System.Windows.Controls;

namespace SyDLab.Usv.Simulator.Presentation.Views
{
	[Export(typeof(IModelBuilderView))]
	public partial class ModelBuilderView : UserControl, IModelBuilderView
	{
		private readonly Lazy<ModelBuilderViewModel> _viewModel;

		public ModelBuilderView()
		{
			InitializeComponent();

			_viewModel = new Lazy<ModelBuilderViewModel>(() => ViewHelper.GetViewModel<ModelBuilderViewModel>(this));
		}

		private void ModelBuilderTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
		}

		private void TreeView_OnSelectionChanged(object sender, EventArgs e)
		{
			if (!_viewModel.IsValueCreated)
				return;

			_viewModel.Value.SelectedObjects.Clear();
        }
	}
}