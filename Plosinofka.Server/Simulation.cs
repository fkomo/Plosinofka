using System;
using System.Diagnostics;
using System.Threading;
using Ujeby.Plosinofka.Engine.Network;
using Ujeby.Plosinofka.Engine.Network.Messages;

namespace Ujeby.Plosinofka.Server
{
	internal class Simulation : IDisposable
	{
		private Thread Thread { get; set; }

		public bool IsRunning { get; private set; } = false;

		public Simulation(bool autoStart = true)
		{
			if (autoStart)
				Start();
		}

		public void Start()
		{
			if (Thread != null)
				Stop();

			Thread = new Thread(Simulate);

			IsRunning = true;
			Thread.Start();
		}

		public void Stop()
		{
			IsRunning = false;
			Thread.Join();
		}

		private void Simulate()
		{
			var maxSimulationDuration = 1000 / 1;

			var lastSimulationDuration = 0;
			var stopwatch = new Stopwatch();
			while (IsRunning)
			{
				stopwatch.Restart();

				SimulationStep(maxSimulationDuration, lastSimulationDuration);

				stopwatch.Stop();

				lastSimulationDuration = (int)stopwatch.ElapsedMilliseconds;

				if (lastSimulationDuration < maxSimulationDuration)
					Thread.Sleep(maxSimulationDuration - lastSimulationDuration);
			}
		}

		private void SimulationStep(int maxSimulationDuration, int lastSimulationDuration)
		{
			try
			{
				// TODO simulate step
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}
		}

		public void MessageReceived(Message message)
		{
			Console.WriteLine(message);
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
