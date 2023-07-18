using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Waf.Applications;
using System.Windows;
using System.Windows.Threading;
using SyDLab.Usv.Simulator.Applications.ViewModels;

namespace SyDLab.Usv.Simulator.Presentation
{
	/// <summary>
	///     App.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class App
	{
		private AggregateCatalog _catalog;
		private CompositionContainer _container;
		private IEnumerable<IModuleController> _moduleControllers;


		public App()
		{
			var profileRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				ApplicationInfo.ProductName, "ProfileOptimization");
			Directory.CreateDirectory(profileRoot);
			ProfileOptimization.SetProfileRoot(profileRoot);
			ProfileOptimization.StartProfile("Startup.profile");
		}


		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			DispatcherUnhandledException += AppDispatcherUnhandledException;
			AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;

			_catalog = new AggregateCatalog();
			// Add the WpfApplicationFramework assembly to the catalog
			_catalog.Catalogs.Add(new AssemblyCatalog(typeof(ViewModel).Assembly));
			// Add the Waf.BookLibrary.Library.Presentation assembly to the catalog
			_catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
			// Add the Waf.BookLibrary.Library.Applications assembly to the catalog
			_catalog.Catalogs.Add(new AssemblyCatalog(typeof(ShellViewModel).Assembly));

			_container = new CompositionContainer(_catalog, CompositionOptions.DisableSilentRejection);
			var batch = new CompositionBatch();
			batch.AddExportedValue(_container);
			_container.Compose(batch);

			_moduleControllers = _container.GetExportedValues<IModuleController>();
			foreach (var moduleController in _moduleControllers)
			{
				moduleController.Initialize();
			}
			foreach (var moduleController in _moduleControllers)
			{
				moduleController.Run();
			}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			foreach (var moduleController in _moduleControllers.Reverse())
			{
				moduleController.Shutdown();
			}
			_container.Dispose();
			_catalog.Dispose();

			base.OnExit(e);
		}

		private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			HandleException(e.Exception, false);
		}

		private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			HandleException(e.ExceptionObject as Exception, e.IsTerminating);
		}

		private static void HandleException(Exception e, bool isTerminating)
		{
			if (e == null)
			{
				return;
			}

			Trace.TraceError(e.ToString());

			if (!isTerminating)
			{
			}
		}

    }
}