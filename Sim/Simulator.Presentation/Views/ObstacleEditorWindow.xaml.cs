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
using SyDLab.Usv.Simulator.Applications.ViewModels;
using SyDLab.Usv.Simulator.Applications.Views;
using SyDLab.Usv.Simulator.Domain.Models;

namespace SyDLab.Usv.Simulator.Presentation.Views
{
    /// <summary>
    /// ObstacleEditorWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    [Export(typeof(IObstacleEditorView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ObstacleEditorWindow : IObstacleEditorView
    {
		public ObstacleEditorWindow()
        {
            InitializeComponent();
        }

        public void Show(object owner)
        {
            Owner = owner as Window;
            Show();
        }

        private bool? private_dialog_result;

        private delegate void FHideWindow();

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            private_dialog_result = DialogResult;
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new
                ObstacleEditorWindow.FHideWindow(HideThisWindow));

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

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (DataContext as ObstacleEditorViewModel).SelectedPoints.Clear();
            foreach (object item in dataGridPoints.SelectedItems)
            {
                if (item is Waypoint)
                    (DataContext as ObstacleEditorViewModel).SelectedPoints.Add(item as Waypoint);
            }
        }
    }
}
