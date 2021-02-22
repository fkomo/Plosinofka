using System;
using Ujeby.Plosinofka.Engine.Network;

namespace Ujeby.Plosinofka.Server
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				var port = 4949; 

				Console.Write($"starting server at localhost:{ port } ... ");
				using (var simulation = new Simulation())
				{
					using (var listener = new Listener(port) { ReceivedMessageHandler = simulation.MessageReceived })
					{
						Console.WriteLine($"OK");

						Console.ReadKey();
						Console.Write("stopping server ... ");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			finally
			{
				Console.WriteLine("OK");
			}
		}
	}
}
