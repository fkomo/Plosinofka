using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Ujeby.Plosinofka.Engine.Network.Messages;

namespace Ujeby.Plosinofka.Engine.Network
{
	public class Listener : IDisposable
	{
		private Thread Thread { get; set; }

		public bool IsRunning { get; private set; } = false;

		public Action<Message> ReceivedMessageHandler { get; set; }

		public int Port { get; private set; }

		public Listener(int port, bool autoStart = true)
		{
			Port = port;

			if (autoStart)
				Start();
		}

		public void Start()
		{
			if (Thread != null)
				Stop();

			Thread = new Thread(Listen);

			IsRunning = true;
			Thread.Start();
		}

		public void Stop()
		{
			IsRunning = false;
			Thread.Join();
		}

		private UdpClient UdpClient = null;

		private void Listen()
		{
			using (UdpClient = new UdpClient(Port))
			{
				UdpClient.Client.ReceiveTimeout = 1000;

				var remoteEndpoint = new IPEndPoint(IPAddress.Any, Port);

				while (IsRunning)
				{
					try
					{
						var bytes = UdpClient.Receive(ref remoteEndpoint);

						var message = Message.Deserialize(bytes);
						if (message != null)
						{
							message.From = new ClientDescriptor(remoteEndpoint.Address, remoteEndpoint.Port);
							ReceivedMessageHandler(message);
						}
					}
					catch (SocketException ex) when (ex.ErrorCode == 10060)
					{
						// timeout
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.ToString());
					}
				}
			}
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
