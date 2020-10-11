using System.Collections.Generic;
using System.Linq;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka.Core
{
	abstract class Simulation
	{
		/// <summary>
		/// desired number of updates per second
		/// </summary>
		public const int GameSpeed = 25;
		public static Vector2f Gravity = new Vector2f(0.0, -5);

		public double LastUpdateDuration { get; protected set; }

		public List<Entity> Entities { get; protected set; } = new List<Entity>();

		public Camera Camera { get; protected set; }

		internal Player GetPlayerEntity() => Entities.SingleOrDefault(e => e is Player) as Player;

		public abstract void Update();

		public abstract void Render(double interpolation);

		public abstract void Destroy();
	}
}
