using System.Waf.Applications;

namespace SyDLab.Usv.Simulator.Applications.Views
{
    public interface IObstacleEditorView : IView
    {
        void Show(object owner);
        void Close();
    }
}