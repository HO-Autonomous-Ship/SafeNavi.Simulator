using System;
using System.ComponentModel;
using System.Waf.Foundation;

namespace SyDLab.Usv.Simulator.Domain.Logs
{
	public class LogItem : Model
	{
		private static uint _count;

		/// <summary>
		///     Index
		/// </summary>
		private uint _index;

		/// <summary>
		///     Message
		/// </summary>
		private string _message;

		/// <summary>
		///     Time
		/// </summary>
		private DateTime _time;

		public LogItem(string message = "")
		{
			Index = ++_count;
			Message = message;
			Time = DateTime.Now;
		}

		[DisplayName("Index")]
		public uint Index
		{
			get { return _index; }
			set { SetProperty(ref _index, value); }
		}

		[DisplayName("Time")]
		public DateTime Time
		{
			get { return _time; }
			set { SetProperty(ref _time, value); }
		}

		[DisplayName("Message")]
		public string Message
		{
			get { return _message; }
			set { SetProperty(ref _message, value); }
		}
	}
}