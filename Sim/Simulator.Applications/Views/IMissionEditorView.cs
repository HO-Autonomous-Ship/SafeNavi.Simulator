using System.Waf.Applications;

namespace SyDLab.Usv.Simulator.Applications.Views
{
	public interface IMissionEditorView : IView
	{
		void ShowDialog(object owner);
		void Close();
	}
}