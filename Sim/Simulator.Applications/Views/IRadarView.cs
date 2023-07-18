using System.Drawing;
using System.Waf.Applications;

namespace SyDLab.Usv.Simulator.Applications.Views
{
    public interface IRadarView : IView
    {
        void UpdateImage(Bitmap image);
    }
}