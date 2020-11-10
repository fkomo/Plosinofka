using Ujeby.Plosinofka.Engine.Entities;

namespace Ujeby.Plosinofka.Engine.Core
{
	public abstract class Simulation
	{
		public static Simulation Instance { get; internal set; }

		/// <summary>desired number of updates per second</summary>
		public abstract int GameSpeed { get; protected set; }
		public abstract double LastUpdateDuration { get; protected set; }

		public Player Player { get; protected set; }
		public Camera Camera { get; protected set; }

		public abstract void Destroy();
		public abstract void Initialize();
		public abstract void Update();
		public abstract void Render(double interpolation);

		public abstract void AddEntity(Entity entity);
	}
}
