
namespace Ujeby.Plosinofka.Engine.Network.Messages
{
	public class LoadLevel : Message
	{
		public override MessageType MessageType => MessageType.LoadLevel;

		[MessageField]
		public string LevelName { get; set; }

		public LoadLevel() : base()
		{
		}

		public LoadLevel(string levelName) : this()
		{
			LevelName = levelName;
		}

		public override string ToString()
		{
			return $"{ base.ToString() }[{ nameof(LoadLevel) };{ LevelName }]";
		}
	}
}
