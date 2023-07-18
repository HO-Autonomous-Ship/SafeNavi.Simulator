using System.ComponentModel.Composition;
using System.Windows;
using Fluent;
using SyDLab.Usv.Simulator.Applications.ViewModels;
using SyDLab.Usv.Simulator.Applications.Views;

namespace SyDLab.Usv.Simulator.Presentation.Views
{
	[Export(typeof(IShellView))]
	public partial class ShellWindow : RibbonWindow, IShellView
	{
		public ShellWindow()
		{
			InitializeComponent();
		}
        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Save project before you quit?", "Exit", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                (DataContext as ShellViewModel).CommandSaveProject.Execute(null);
                e.Cancel = false;
            }
            else if (result == MessageBoxResult.No)
            {
                e.Cancel = false;
            }
            else
                e.Cancel = true;
        }
    }
}
