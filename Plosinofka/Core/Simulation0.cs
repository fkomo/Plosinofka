using System;
using System.Collections.Generic;
using System.Linq;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Entities;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Entities;
using Ujeby.Plosinofka.Game.Graphics;

namespace Ujeby.Plosinofka.Game
{
	public class Simulation0 : Simulation
	{
		public override int GameSpeed { get; protected set; } = 50;
		public override double LastUpdateDuration { get; protected set; }

		public static Vector2f Gravity = new Vector2f(0.0, -1);
		private const double TerminalFallingVelocity = -8;

		private readonly List<Entity> Entities = new List<Entity>();

		private Level CurrentLevel;

		private readonly DebugData DebugData = new DebugData();

		public Simulation0()
		{
		}

		public override void Initialize()
		{
			// create player
			Player = new Player0("ujeb");

			LoadLevel("Level0");
		}

		public override void Destroy()
		{
			Log.Add($"Simulation0.Destroy()");
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
			var view = Vector2i.FullHD / 4;
			//Camera = new RoomFixedCamera(view);
			var camWindowSize = view / 3;
			Camera = new WindowCamera(view, new AABB(Vector2f.Zero, camWindowSize) + camWindowSize);
			//{
			//	PlatformSnapping = true
			//};
		}

		public override void Update()
		{
			var start = Engine.Core.Game.GetElapsed();

			// remove obsolete entities
			for (var i = Entities.Count - 1; i >= 0; i--)
				if ((Entities[i] as IDestroyable)?.Obsolete() == true)
					Entities.RemoveAt(i);

			// update all entities
			for (var i = Entities.Count - 1; i >= 0; i--)
				Entities[i].Update();

			// player standing on obstacle (moving platform)
			if (Player.ObstacleAt(Side.Down))
			{
				var bb = Player.BoundingBox + Player.Position;
				var underPlayer = new AABB(new Vector2f(bb.Left + 1, bb.Bottom - 1), new Vector2f(bb.Right - 1, bb.Bottom));
				if (Entities.SingleOrDefault(e => e is Platform p && underPlayer.Overlap(p.BoundingBox + p.Position)) is Platform platformDown)
					Player.Velocity += platformDown.Velocity;
			}

			// platform pushing player
			var obstacles = Entities.Where(e => (e as Obstacle) != null).Select(o => o as Obstacle).ToArray();
			foreach (var obstacle in obstacles)
			{
				var playerAABB = Player.BoundingBox + Player.Position;
				if (obstacle is Platform platform)
				{
					var platformAABB = platform.BoundingBox + platform.Position;
					if (playerAABB.Overlap(platformAABB))
					{
						var overlap = AABB.Overlap(playerAABB, platformAABB);
						Player.Velocity += overlap.Size * platform.Velocity.Normalize();
					}
				}
			}

			// solve collisions of dynamic entities
			foreach (var entity in Entities)
			{
				if (entity is DynamicEntity dynamicEntity)
				{
					// if entity is oblivious to environment solve its collisions
					if (dynamicEntity.Responsive)
					{
						Solve(dynamicEntity, obstacles, out Vector2f position, out Vector2f velocity);
						dynamicEntity.Position = position;
						dynamicEntity.Velocity = velocity;
					}
					else
						dynamicEntity.Position += dynamicEntity.Velocity;

					//if (entity as Player != null || entity as Platform != null)
					//	Log.Add(entity.ToString());
				}
			}

			foreach (var entity in Entities)
				DebugData.TrackEntity(entity as ITrack);

			// update player awarness of surrounding objects
			Player.UpdateSurroundings(obstacles);

			// update camera with respect to world/level borders
			Camera.Update(Player, new AABB(Vector2f.Zero, CurrentLevel.Size));

			LastUpdateDuration = Engine.Core.Game.GetElapsed() - start;
		}

		public override void Render(double interpolation)
		{
			// viewport
			var view = Camera.InterpolatedView(interpolation);

			// parallax scrolling
			var parallax = view.Min / (CurrentLevel.Size - view.Size);

			var lights = Entities.Where(e => e is Light).Select(e => e as Light).ToArray();
			var obstacles = 
				Entities.Where(e => (e as Obstacle) != null).Select(e => (e as Obstacle).BoundingBox + e.Position)
				.Concat(new [] { Player.BoundingBox + Player.InterpolatedPosition(interpolation) })
				.ToArray();

			// render all layers, back to front
			foreach (var layer in CurrentLevel.Layers)
			{
				// main layer
				if (layer.Depth == 0)
				{
					// background layer
					Renderer.Instance.RenderLayer(view, layer, lights, obstacles);

					// entities
					foreach (var entity in Entities)
						(entity as IRender)?.Render(view, interpolation);
				}
				else
					// background / foreground layers (with possible parallax scrolling)
					Renderer.Instance.RenderLayer(!layer.Parallax ? 
						view : (new AABB(Vector2f.Zero, view.Size) + (layer.Size - view.Size) * parallax), 
						layer);
			}

			// debug
			DebugData.Render(view, interpolation, Entities.ToArray());

			if (Settings.Instance.GetDebug(DebugSetting.DrawCamera))
				(Camera as IRender)?.Render(view, interpolation);

			// gui is always on top
			Gui.Instance.Render(view, interpolation);
		}

		public override void AddEntity(Entity entity)
		{
			Entities.Add(entity);
			DebugData.TrackEntity(entity as ITrack);
		}

		public bool Solve(DynamicEntity entity, Obstacle[] obstacles, out Vector2f position, out Vector2f velocity)
		{
			position = entity.Position;
			velocity = entity.Velocity;

			// limit falling velocity
			velocity.Y = Math.Max(velocity.Y, TerminalFallingVelocity);

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
				var t = Collisions.Trace(entityBox, velocity, obstacles.Select(o => o.BoundingBox + o.Position).ToArray(), out Vector2f normal);
				if (t.LeEq(distance))
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

					if (velocity == Vector2f.Zero) // or better just close to zero ?
						break; // solved - nowhere to move
				}
				else
					break; // solved - nothing else stands in the way
			}

			position += velocity;
			velocity = remainingVelocity;

			return collisionFound;
		}
	}
}
