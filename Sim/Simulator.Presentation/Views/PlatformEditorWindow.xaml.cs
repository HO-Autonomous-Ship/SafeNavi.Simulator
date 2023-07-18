using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using SyDLab.Usv.Simulator.Applications.Views;

namespace SyDLab.Usv.Simulator.Presentation.Views
{
	/// <summary>
	/// PlatformEditorWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	[Export(typeof(IPlatformEditorView)), PartCreationPolicy(CreationPolicy.NonShared)]
	public partial class PlatformEditorWindow : IPlatformEditorView
	{
		public PlatformEditorWindow()
		{
			InitializeComponent();
		}
		
		public void ShowDialog(object owner)
		{
			Owner = owner as Window;
			ShowDialog();
		}

        private bool? private_dialog_result;

        private delegate void FHideWindow();


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            private_dialog_result = DialogResult;
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new
                PlatformEditorWindow.FHideWindow(HideThisWindow));

            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.Activate();
        }

        private void HideThisWindow()
        {
            this.Hide();
            (typeof(Window)).GetField("_isClosing", BindingFlags.Instance |
                                                    BindingFlags.NonPublic).SetValue(this, false);
            (typeof(Window)).GetField("_dialogResult", BindingFlags.Instance |
                                                       BindingFlags.NonPublic).SetValue(this, private_dialog_result);
            private_dialog_result = null;
        }
	}
}
