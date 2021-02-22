using Ujeby.Plosinofka.Engine.Network.Messages;

namespace Ujeby.Plosinofka.Engine.Network
{
	public class IncommingMessage
	{
		public ClientDescriptor Sender { get; set; }
		public ClientDescriptor Reciever { get; set; }
		public Message Message { get; set; }
	}

	public class MessageQueue
	{
		//public Add(IncommingMessage incommingMessage)
		//{

		//}
	}
}
