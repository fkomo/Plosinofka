using System.Collections.Generic;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka.Core
{
	public abstract class Simulation
	{
		/// <summary>
		/// desired number of updates per second
		/// </summary>
		public const int GameSpeed = 50;
		public static Vector2f Gravity = new Vector2f(0.0, -2);
		public static double TerminalFallingVelocity = -16;

		public double LastUpdateDuration { get; protected set; }

		public List<DynamicEntity> DynamicEntities { get; protected set; } = new List<DynamicEntity>();
		public List<Entity> StaticEntities { get; protected set; } = new List<Entity>();
		public Dictionary<string, List<Vector2f>> EntityHistory { get; protected set; } = new Dictionary<string, List<Vector2f>>();

		public Player Player { get; protected set; }
		public Camera Camera { get; protected set; }

		public abstract void Update();

		public abstract void Render(double interpolation);

		public abstract void Destroy();
	}
}
