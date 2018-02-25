using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit.Util;

namespace MassTransit.EventStoreIntegration.Sample
{
	class Program
	{
		static void Main(string[] args)
		{
			var test = new Sample();

			try
			{
				TaskUtil.Await(() => test.Execute());

				Console.ReadLine();
			}
			catch (Exception x)
			{
				Console.WriteLine(x);
				Console.ReadLine();
			}
			finally
			{
				test.Stop();
			}
		}
	}
}
