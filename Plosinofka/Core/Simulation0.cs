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

		private const int FadeMax = 255;
		private const int FadeMin = 0;
		private const int FadeStep = 5;

		private int FadeFrame = FadeMin;
		private bool FadeAnimation = false;

		public Simulation0()
		{
		}

		public override void Initialize()
		{
			LoadLevel("Level0");
		}

		public override void Destroy()
		{
			Log.Add($"Simulation0.Destroy()");
		}

		private void LoadLevel(string levelName)
		{
			// create player
			Player = new Player0("ujeb");

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

			FadeAnimation = true;
			FadeFrame = FadeMax;
		}

		public override void Update()
		{
			var start = Engine.Core.Game.GetElapsed();

			// TODO simulation state machine (paused game / menu | game | level loading | fade in/out? animation)
			
			if (FadeAnimation)
			{
				FadeFrame += Player.Alive ? -FadeStep : FadeStep;

				if (FadeFrame < FadeMin || FadeMax < FadeFrame)
				{
					FadeFrame = Math.Min(FadeMax, Math.Max(FadeMin, FadeFrame));
					FadeAnimation = false;

					if (!Player.Alive)
					{
						// TODO reload current level
						LoadLevel("Level0");
					}
				}
			}

			if (Player.Alive)
			{
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
							Solve(dynamicEntity, out Vector2f position, out Vector2f velocity);
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

				// update camera with respect to world/level borders
				Camera.Update(Player, new AABB(Vector2f.Zero, CurrentLevel.Size));

				// if player is still alive
				if (!CheckPlayerDeath())
				{
					// update player awarness of surrounding objects
					Player.UpdateSurroundings(Entities.Where(e => (e as Obstacle) != null).Select(o => o as Obstacle).ToArray());

					if (CurrentLevel.Finish.Overlap(Player.BoundingBox + Player.Position))
					{
						// TODO move to next level
						LoadLevel("Level0");
					}
				}
			}

			LastUpdateDuration = Engine.Core.Game.GetElapsed() - start;
		}

		public override void Render(double interpolation)
		{
			// viewport
			var view = Camera.InterpolatedView(interpolation);

			if (Player.Alive || FadeAnimation)
			{
				// parallax scrolling
				var parallax = view.Min / (CurrentLevel.Size - view.Size);

				var lights = Entities.Where(e => e is Light).Select(e => e as Light).ToArray();
				var obstacles =
					Entities.Where(e => (e as Obstacle) != null).Select(e => (e as Obstacle).BoundingBox + e.Position)
					.Concat(new[] { Player.BoundingBox + Player.InterpolatedPosition(interpolation) })
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

				if (FadeAnimation)
					Renderer.Instance.RenderRectangleOverlay(view, new AABB(Vector2f.Zero, view.Size), new Color4b(0, 0, 0, (byte)FadeFrame));
			}

			// gui is always on top
			Gui.Instance.Render(view, interpolation);
		}

		private bool CheckPlayerDeath()
		{
			if (Entities.Any(e => e is DeathZone deathZone && deathZone.VsEntity(Player)) || (Player.Position.Y + Player.BoundingBox.Min.Y) < 0)
			{
				// player death
				Player.Die();

				Camera.Update(Player, new AABB(Vector2f.Zero, CurrentLevel.Size));

				FadeAnimation = true;
				FadeFrame = FadeMin;

				return true;
			}

			return false;
		}

		public override void AddEntity(Entity entity)
		{
			Entities.Add(entity);
			DebugData.TrackEntity(entity as ITrack);
		}

		public bool Solve(DynamicEntity entity, out Vector2f position, out Vector2f velocity)
		{
			position = entity.Position;
			velocity = entity.Velocity;

			// limit falling velocity
			velocity.Y = Math.Max(velocity.Y, TerminalFallingVelocity);

			// no velocity ? no collision!
			if (velocity == Vector2f.Zero)
				return false;

			var obstacles = Entities.Where(e => (e as Obstacle) != null).Select(o => o as Obstacle).ToArray();

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
