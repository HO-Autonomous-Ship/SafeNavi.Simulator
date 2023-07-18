using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace SyDLab.Usv.Simulator.Domain.Logs
{
	public class LogManager
	{
		public LogManager()
		{
			LogItems.CollectionChanged += LogItemsOnCollectionChanged;

			AddLog(string.Empty);
			AddLog("Log started.");
			AddLog(string.Empty);
			AddLog(string.Empty);
		}

		public LogItem LastLogItem => LogItems.LastOrDefault();

		public ObservableCollection<LogItem> LogItems { get; } = new ObservableCollection<LogItem>();

		public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

		public bool CanMultiline { get; set; }

		private void LogItemsOnCollectionChanged(object sender,
			NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			switch (notifyCollectionChangedEventArgs.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (LogItem item in notifyCollectionChangedEventArgs.NewItems)
						Messages.Add(item.Message);
					break;
				case NotifyCollectionChangedAction.Remove:
					break;
				case NotifyCollectionChangedAction.Replace:
					break;
				case NotifyCollectionChangedAction.Move:
					break;
				case NotifyCollectionChangedAction.Reset:
					Messages.Clear();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void AddLog(string logMessage)
		{
			if (LogItems.Count == 0)
			{
				if (CanMultiline)
				{
					var lines = logMessage.Split(Environment.NewLine.ToCharArray());
					foreach (var item in lines)
						LogItems.Add(new LogItem(item));
				}
				else
				{
					var tmp = logMessage.Replace(Environment.NewLine, "");
					LogItems.Add(new LogItem(tmp));
				}

			}
			else
			{
				if (CanMultiline)
				{
					var lines = logMessage.Split(Environment.NewLine.ToCharArray());
					foreach (var item in lines)
						LogItems.Insert(0, new LogItem(item));
				}
				else
				{
					var tmp = logMessage.Replace(Environment.NewLine, "");
					LogItems.Insert(0, new LogItem(tmp));
				}
			}
				//LogItems.Insert(0, new LogItem(logMessage));
		}

		public void WriteLine(IEnumerable<string> args)
		{
			foreach (var item in args)
				AddLog(item);
		}

		public void WriteLine(string format, object arg)
		{
			AddLog(arg != null ? string.Format(format, arg) : format);
		}

		public void WriteLine(string format, params object[] args)
		{
			if (args != null && args.Length > 0)
				AddLog(string.Format(format, args));
			else
				AddLog(format);
		}
	}
}