using SyDLab.Usv.Simulator.Applications.Views;
using System.ComponentModel.Composition;

namespace SyDLab.Usv.Simulator.Presentation.Views
{
    [Export(typeof(IMonitorView))]
    public partial class MonitorView : IMonitorView
    {
        public MonitorView()
        {
            InitializeComponent();
        }
    }
}
