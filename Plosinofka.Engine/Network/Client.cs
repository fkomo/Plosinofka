using System;
using System.Net;
using System.Net.Sockets;
using Ujeby.Plosinofka.Engine.Network.Messages;

namespace Ujeby.Plosinofka.Engine.Network
{
	public class Client : IDisposable
	{
		public static Client Instance { get; private set; } = new Client();

		public Guid SessionId { get; private set; }
		public Guid ClientId { get; private set; } = Guid.NewGuid();

		Socket Socket { get; set; }
		IPEndPoint ServerEndpoint { get; set; }

		public Client()
		{
			SessionId = Guid.NewGuid();

			Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			ServerEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4949);
		}

		public bool Send(Message message)
		{
			try
			{
				if (Socket == null)
					throw new ObjectDisposedException(nameof(Socket));

				message.ClientId = ClientId;
				message.SessionId = SessionId;

				Socket.SendTo(Message.Serialize(message), ServerEndpoint);

				return true;
			}
			catch (Exception ex)
			{
				// TODO error
			}

			return false;
		}

		public void Dispose()
		{
			Socket.Dispose();
			Socket = null;
		}
	}
}
