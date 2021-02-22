
namespace Ujeby.Plosinofka.Engine.Network.Messages
{
	public class LeaveGame : Message
	{
		public override MessageType MessageType { get { return MessageType.LeaveGame; } }

		[MessageField]
		public string Login { get; set; }

		public LeaveGame() : base()
		{
		}

		public override string ToString()
		{
			return $"{ base.ToString() }[{ nameof(LeaveGame) };{ Login }]";
		}
	}
}
