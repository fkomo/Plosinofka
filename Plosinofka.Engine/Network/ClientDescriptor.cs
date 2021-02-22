using System.Net;

namespace Ujeby.Plosinofka.Engine.Network
{
	public struct ClientDescriptor
	{
		public IPAddress Address;
		public int Port;

		public ClientDescriptor(IPAddress address, int port) : this()
		{
			Address = address;
			Port = port;
		}

		public override string ToString()
		{
			return $"[{ Address }:{ Port }]";
		}
	}
}
