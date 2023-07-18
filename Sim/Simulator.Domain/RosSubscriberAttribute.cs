using System;

namespace SyDLab.Usv.Simulator.Domain
{
	public class RosSubscriberAttribute : Attribute
	{
		public RosSubscriberAttribute(string topic, string messageType, string messageRegex)
		{
			Topic = topic;
			MessageType = messageType;
			MessageRegex = messageRegex;
		}

		public string MessageRegex { get; private set; }
		public string MessageType { get; private set; }
		public string Topic { get; private set; }
	}
}