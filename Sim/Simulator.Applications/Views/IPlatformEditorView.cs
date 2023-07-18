using System.Waf.Applications;

namespace SyDLab.Usv.Simulator.Applications.Views
{
	public interface IPlatformEditorView : IView
	{
		void ShowDialog(object owner);
		void Close();
	}
}