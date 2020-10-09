using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	class Simulation
	{
		/// <summary>
		/// desired number of game updates per second
		/// </summary>
		public int GameSpeed { get; internal set; } = 25;
		public static Vector2f Gravity = new Vector2f(0.0, 5);

		public double LastUpdateDuration { get; internal set; }

		private Stopwatch Stopwatch = new Stopwatch();
		private readonly Random Rng = new Random();

		public World World { get; private set; }
		public List<Entity> Entities { get; private set; } = new List<Entity>();
		public Entity[] EntitiesBeforeUpdate { get; private set; } = null;

		public Simulation()
		{
			Initialize();
		}

		private void Initialize()
		{
			World = new World(1080 / 8 * 3);

			var player = new Player("Jebko");
			player.Position = new Vector2f(1920 / 4, World.Ground - player.Size.Height / 2);

			Entities.Add(player);
		}

		public void Update()
		{
			Stopwatch.Restart();

			EntitiesBeforeUpdate = Entities.Select(e => e.Copy()).ToArray();

			foreach (var entity in Entities)
				entity.Update();

			Stopwatch.Stop();
			LastUpdateDuration = Game.GetElapsed(Stopwatch);
		}

		public void Destroy()
		{

		}

		internal Player GetPlayerEntity()
		{
			return Entities.Single(e => e is Player) as Player;
		}
	}
}
