
namespace Ujeby.Plosinofka.Engine.Core
{
	public abstract class Input
	{
		public static Input Instance { get; internal set; }

		public abstract bool Handle(Simulation simulation);
	}
}
