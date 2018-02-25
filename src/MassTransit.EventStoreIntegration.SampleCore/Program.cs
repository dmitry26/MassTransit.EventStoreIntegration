using System;
using MassTransit.Util;

namespace MassTransit.EventStoreIntegration.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
			try
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
			catch (Exception x)
			{
				Console.WriteLine(x);
				Console.ReadLine();
			}
		}
    }
}
