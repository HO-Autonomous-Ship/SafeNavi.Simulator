using System.Waf.Applications;

namespace SyDLab.Usv.Simulator.Applications.Views
{
    public interface ISettingView : IView
    {
        void Show(object owner);

        void Close();
    }
}
