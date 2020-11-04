using System.Collections.Generic;
using System.Linq;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka
{
	public class Simulation : Singleton<Simulation>, ICollisionSolver
	{
		/// <summary>
		/// desired number of updates per second
		/// </summary>
		public const int GameSpeed = 50;
		public static Vector2f Gravity = new Vector2f(0.0, -1);
		public static double TerminalFallingVelocity = -16;

		public double LastUpdateDuration { get; protected set; }

		protected List<Entity> Entities = new List<Entity>();

		public Player Player { get; protected set; }
		public Camera Camera { get; protected set; }

		private Level CurrentLevel;

		private DebugData DebugData = new DebugData();

		public Simulation()
		{
			// create player
			Player = new Player("player1");
		}

		public void Initialize()
		{
			LoadLevel("Level0");
		}

		private void LoadLevel(string levelName)
		{
			// clear old entities
			Entities.Clear();
			DebugData.Clear();

			// load level
			CurrentLevel = Level.Load(levelName);

			// set player position
			Player.Position = CurrentLevel.Start;
			AddEntity(Player);

			// make camera view smaller then window size for more pixelated look!
			Camera = new RoomCamera(Vector2i.FullHD / 4);
		}

		public void Update()
		{
			var start = Game.GetElapsed();

			// remove obsolete entities
			for (var i = Entities.Count - 1; i >= 0; i--)
				if ((Entities[i] as IDestroyable)?.Obsolete() == true)
					Entities.RemoveAt(i);

			// update all entities
			for (var i = Entities.Count - 1; i >= 0; i--)
				Entities[i].Update(CurrentLevel);

			// solve collisions of dynamic entities
			foreach (var entity in Entities)
			{
				if (entity is DynamicEntity dynamicEntity)
				{
					Solve(dynamicEntity, out Vector2f position, out Vector2f velocity);
					dynamicEntity.Position = position;
					dynamicEntity.Velocity = velocity;

					DebugData.TrackEntity(entity as ITrackable);
				}
			}

			// update camera with respect to world/level borders
			Camera.Update(Player, CurrentLevel.Size);

			LastUpdateDuration = Game.GetElapsed() - start;
		}

		public void Render(double interpolation)
		{
			// viewport
			var view = Camera.InterpolatedView(interpolation);

			// parallax scrolling
			var parallax = view.Min / (CurrentLevel.Size - view.Size);

			// render all layers, back to front
			foreach (var layer in CurrentLevel.Layers)
			{
				// main layer
				if (layer.Depth == 0)
				{
					var obstacles = CurrentLevel.Obstacles.ToList();
					obstacles.Add(Player.BoundingBox + Player.InterpolatedPosition(interpolation));

					// background layer
					Renderer.Instance.RenderLayer(view, layer,
						Entities.Where(e => e is Light).Select(e => e as Light).ToArray(),
						obstacles.ToArray());

					// entities
					foreach (var entity in Entities)
						(entity as IRenderable)?.Render(view, interpolation);
				}
				else
					// background / foreground layers (with possible parallax scrolling)
					Renderer.Instance.RenderLayer(!layer.Parallax ? 
						view : (new AABB(Vector2f.Zero, view.Size) + (layer.Size - view.Size) * parallax), 
						layer);
			}

			// debug
			DebugData.Render(view, interpolation, Entities.ToArray(), CurrentLevel.Obstacles);
		}

		internal void AddEntity(Entity entity)
		{
			Entities.Add(entity);
			DebugData.TrackEntity(entity as ITrackable);
		}

		public bool Solve(DynamicEntity entity, out Vector2f position, out Vector2f velocity)
		{
			position = entity.Position;
			velocity = entity.Velocity;

			// no velocity ? no collision!
			if (velocity == Vector2f.Zero)
				return false;

			var remainingVelocity = velocity;

			var collisionFound = false;
			while (true)
			{
				var distance = velocity.Length();
				var direction = velocity.Normalize();

				var entityBox = entity.BoundingBox + position;
				var t = CurrentLevel.Trace(entityBox, direction, out Vector2f normal);
				if (t <= distance)
				{
					collisionFound = true;

					var hitPosition = position + direction * t;
					
					// set new position & velocity
					velocity = (position + velocity) 
						+ normal * Vector2f.Dot(direction.Inv(), normal) * (distance - t) 
						- hitPosition;
					position = hitPosition;

					if (normal.X == 0)
						remainingVelocity.Y = 0;
					else 
						remainingVelocity.X = 0;

					//Log.Add($"Simulation.CollisionSolved({ entity }, bb={ entityBox }): t={ t }; position={ position }; velocity={ velocity }; remainingVelocity={ remainingVelocity }");

					if (velocity == Vector2f.Zero) // or better just close to zero ?
						break; // solved - nowhere to move
				}
				else
					break; // solved - nothing else stands in the way
			}

			position += velocity;
			velocity = remainingVelocity;

			//Log.Add($"World.Solved({ entity }; position={ entity.Position }; velocity={ entity.Velocity }): position={ position }; velocity={ velocity }");

			return collisionFound;
		}

		public static void Destroy()
		{
			instance = null;
		}
	}
}
