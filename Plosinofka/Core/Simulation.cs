using System.Collections.Generic;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Core
{
	public abstract class Simulation
	{
		/// <summary>
		/// desired number of updates per second
		/// </summary>
		public const int GameSpeed = 50;
		public static Vector2f Gravity = new Vector2f(0.0, -1);
		public static double TerminalFallingVelocity = -16;

		public double LastUpdateDuration { get; protected set; }

		protected List<Entity> Entities = new List<Entity>();

		/// <summary>
		/// Entity.Name vs its TrackedData
		/// </summary>
		protected Dictionary<string, FixedQueue<TrackedData>> TrackedEntities = 
			new Dictionary<string, FixedQueue<TrackedData>>();

		public Player Player { get; protected set; }
		public Camera Camera { get; protected set; }

		public abstract void Update();

		public abstract void Render(double interpolation);
	}
}
