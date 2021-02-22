using System;

namespace Ujeby.Plosinofka.Engine.Network.Messages
{
	public class MessageFieldAttribute : Attribute
	{
		//public int Order { get; set; }

		public bool Ignore { get; set; } = false;
	}
}
