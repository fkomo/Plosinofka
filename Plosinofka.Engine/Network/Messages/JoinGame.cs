
namespace Ujeby.Plosinofka.Engine.Network.Messages
{
	public class JoinGame : Message
	{
		public override MessageType MessageType { get { return MessageType.JoinGame; } }

		[MessageField]
		public string Login { get; set; }

		public JoinGame() : base()
		{
		}

		public override string ToString()
		{
			return $"{ base.ToString() }[{ nameof(JoinGame) };{ Login }]";
		}
	}
}
