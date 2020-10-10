using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	class Simulation
	{
		/// <summary>
		/// desired number of game updates per second
		/// </summary>
		public int GameSpeed { get; internal set; } = 25;
		public static Vector2f Gravity = new Vector2f(0.0, -5);

		public double LastUpdateDuration { get; internal set; }

		private Stopwatch Stopwatch = new Stopwatch();
		private readonly Random Rng = new Random();

		public World World { get; private set; }
		public List<Entity> Entities { get; private set; } = new List<Entity>();

		public Entity[] EntitiesBeforeUpdate { get; private set; } = null;

		public Camera Camera { get; private set; }

		public Simulation()
		{
			Initialize();
		}

		private void Initialize()
		{
			Entities.Add(new Player("Jebko") { Position = new Vector2f(128, 128) });
			//Entities.Add(new TestCube("Test") { Position = new Vector2f(256, 256) });

			World = new World();

			Camera = new Camera(Renderer.Instance.WindowSize / 2, GetPlayerEntity());
		}

		public void Update()
		{
			Stopwatch.Restart();

			// save entities state before update
			EntitiesBeforeUpdate = Entities.Select(e => e.Copy()).ToArray();

			// update all entities
			foreach (var entity in Entities)
				entity.Update();

			// update camera
			Camera.Update(GetPlayerEntity(), World);

			Stopwatch.Stop();
			LastUpdateDuration = Game.GetElapsed(Stopwatch);
		}

		public void Destroy()
		{
		}

		internal Player GetPlayerEntity()
		{
			return Entities.SingleOrDefault(e => e is Player) as Player;
		}
	}
}
