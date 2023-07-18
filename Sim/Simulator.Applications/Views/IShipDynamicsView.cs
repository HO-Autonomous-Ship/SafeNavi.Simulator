using System.Waf.Applications;

namespace SyDLab.Usv.Simulator.Applications.Views
{
    public interface IShipDynamicsView : IView
    {
        void ShowDialog(object owner);
        void Close();
    }
}