using System;
using System.Collections.Generic;
using System.Text;

namespace Dmo.MassTransit
{
	public class RabbitMqOptions
	{
		public string Username { get; set; }

		public string Password { get; set; }

		public ushort? Heartbeat { get; set; }

		public string Host { get; set; }

		public int? Port { get; set; }

		public string VirtualHost { get; set; }

		public string ClusterMembers { get; set; }

		public string ClusterNodeHostname { get; set; }
	}
}
