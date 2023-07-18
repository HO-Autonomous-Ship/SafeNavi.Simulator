using System.ComponentModel;
using SyDLab.Usv.Simulator.Applications.Views;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using SyDLab.Usv.Simulator.Domain.Utils;


namespace SyDLab.Usv.Simulator.Presentation.Views
{
    /// <summary>
    /// SettingView.xaml에 대한 상호 작용 논리
    /// </summary>
    [Export(typeof(ISettingView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class SettingView : Window, ISettingView
    {
        public SettingView()
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
                SettingView.FHideWindow(HideThisWindow));

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

        private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var element = sender as UIElement;
                element?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
    }


}
