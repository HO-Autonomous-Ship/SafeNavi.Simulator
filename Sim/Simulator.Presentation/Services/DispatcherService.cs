using System;
using System.ComponentModel.Composition.Primitives;
using System.Windows;
using System.Windows.Threading;
using SyDLab.Usv.Simulator.Applications.Services;
using System.ComponentModel.Composition;

namespace SyDLab.Usv.Simulator.Presentation.Services
{
	[Export(typeof(IDispatcherService))]
	public class DispatcherService : IDispatcherService
	{
		public void Invoke(Action action)
		{
			var dispatchObject = Application.Current.Dispatcher;
			if (dispatchObject == null || dispatchObject.CheckAccess())
			{
				action();
			}
			else
			{
				dispatchObject.Invoke(action);
			}
		}
	}
}