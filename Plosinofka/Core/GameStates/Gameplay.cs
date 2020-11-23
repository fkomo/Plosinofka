using System;
using System.Linq;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Entities;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Graphics;

namespace Ujeby.Plosinofka.Game.Core
{
	public class Gameplay : GameState
	{
		public override GameStateEnum AsEnum => GameStateEnum.Gameplay;

		private const double TerminalFallingVelocity = -8;

		private const int FadeMax = 255;
		private const int FadeMin = 0;
		private const int FadeStep = 5;

		private int FadeFrame = FadeMin;
		private bool Fade = false;

		private readonly DebugData DebugData = new DebugData();

		// TODO gameplay timer

		public Gameplay()
		{
			Fade = true;
			FadeFrame = FadeMax;
		}

		public override void Update(Game0 game)
		{
			base.Update(game);

			if (Fade)
			{
				FadeFrame += game.Player.Alive ? -FadeStep : FadeStep;

				if (FadeFrame < FadeMin || FadeMax < FadeFrame)
				{
					FadeFrame = Math.Min(FadeMax, Math.Max(FadeMin, FadeFrame));
					Fade = false;

					if (!game.Player.Alive)
						game.ChangeState(new LoadingLevel(game.CurrentLevel.Name));
				}
			}

			if (game.Player.Alive)
			{
				// remove obsolete entities
				for (var i = game.Entities.Count - 1; i >= 0; i--)
					if ((game.Entities[i] as IDestroyable)?.Obsolete() == true)
						game.Entities.RemoveAt(i);

				// update all entities
				for (var i = game.Entities.Count - 1; i >= 0; i--)
					game.Entities[i].Update();

				// player standing on obstacle (moving platform)
				if (game.Player.ObstacleAt(Side.Down))
				{
					var bb = game.Player.BoundingBox + game.Player.Position;
					var underPlayer = new AABB(new Vector2f(bb.Left + 1, bb.Bottom - 1), new Vector2f(bb.Right - 1, bb.Bottom));
					if (game.Entities.SingleOrDefault(e => e is Platform p && underPlayer.Overlap(p.BoundingBox + p.Position)) is Platform platformDown)
						game.Player.Velocity += platformDown.Velocity;
				}

				// platform pushing player
				var obstacles = game.Entities.Where(e => (e as Obstacle) != null).Select(o => o as Obstacle).ToArray();
				foreach (var obstacle in obstacles)
				{
					var playerAABB = game.Player.BoundingBox + game.Player.Position;
					if (obstacle is Platform platform)
					{
						var platformAABB = platform.BoundingBox + platform.Position;
						if (playerAABB.Overlap(platformAABB))
						{
							var overlap = AABB.Overlap(playerAABB, platformAABB);
							game.Player.Velocity += overlap.Size * platform.Velocity.Normalize();
						}
					}
				}

				// solve collisions of dynamic entities
				foreach (var entity in game.Entities)
				{
					if (entity is DynamicEntity dynamicEntity)
					{
						// if entity is oblivious to environment solve its collisions
						if (dynamicEntity.Responsive)
						{
							Solve(game, dynamicEntity, out Vector2f position, out Vector2f velocity);
							dynamicEntity.Position = position;
							dynamicEntity.Velocity = velocity;
						}
						else
							dynamicEntity.Position += dynamicEntity.Velocity;

						//if (entity as Player != null || entity as Platform != null)
						//	Log.Add(entity.ToString());
					}
				}

				foreach (var entity in game.Entities)
					DebugData.TrackEntity(entity as ITrack);

				// update camera with respect to world/level borders
				game.Camera.Update(game.Player, new AABB(Vector2f.Zero, game.CurrentLevel.Size));

				// if player is still alive
				if (!CheckPlayerDeath(game))
				{
					// update player awarness of surrounding objects
					game.Player.UpdateSurroundings(game.Entities.Where(e => (e as Obstacle) != null).Select(o => o as Obstacle).ToArray());

					if (game.Entities.SingleOrDefault(e => e is LevelEndZone) is LevelEndZone levelEndZone && levelEndZone.VsEntity(game.Player))
						game.ChangeState(new LoadingLevel(game.CurrentLevel.NextLevel));
				}
			}
		}

		public override void Render(Game0 game, double interpolation)
		{
			base.Render(game, interpolation);
			
			var view = game.Camera.InterpolatedView(interpolation);

			if (game.Player.Alive || Fade)
			{
				// parallax scrolling
				var parallax = view.Min / (game.CurrentLevel.Size - view.Size);

				var lights = game.Entities.Where(e => e is Light).Select(e => e as Light).ToArray();
				var obstacles =
					game.Entities.Where(e => (e as Obstacle) != null).Select(e => (e as Obstacle).BoundingBox + e.Position)
					.Concat(new[] { game.Player.BoundingBox + game.Player.InterpolatedPosition(interpolation) })
					.ToArray();

				// render all layers, back to front
				foreach (var layer in game.CurrentLevel.Layers)
				{
					// main layer
					if (layer.Depth == 0)
					{
						// background layer
						Renderer.Instance.RenderLayer(view, layer, lights, obstacles);

						// entities
						foreach (var entity in game.Entities)
							(entity as IRender)?.Render(view, interpolation);
					}
					else
						// background / foreground layers (with possible parallax scrolling)
						Renderer.Instance.RenderLayer(!layer.Parallax ?
							view : (new AABB(Vector2f.Zero, view.Size) + (layer.Size - view.Size) * parallax),
							layer);
				}

				// debug
				DebugData.Render(view, interpolation, game.Entities.ToArray());

				if (Settings.Instance.GetDebug(DebugSetting.DrawCamera))
					(game.Camera as IRender)?.Render(view, interpolation);

				if (Fade)
					Renderer.Instance.RenderRectangleOverlay(view, new AABB(Vector2f.Zero, view.Size), new Color4b(0, 0, 0, (byte)FadeFrame));
			}

			// gameplay gui, always on top
			Gui.Instance.Render(view, interpolation);
		}

		private bool CheckPlayerDeath(Game0 game)
		{
			if (game.Entities.Any(e => e is DeathZone deathZone && deathZone.VsEntity(game.Player)) || 
				(game.Player.Position.Y + game.Player.BoundingBox.Min.Y) < 0)
			{
				// player death
				game.Player.Die();

				game.Camera.Update(game.Player, new AABB(Vector2f.Zero, game.CurrentLevel.Size));

				Fade = true;
				FadeFrame = FadeMin;

				return true;
			}

			return false;
		}

		private bool Solve(Game0 game, DynamicEntity entity, out Vector2f position, out Vector2f velocity)
		{
			position = entity.Position;
			velocity = entity.Velocity;

			// limit falling velocity
			velocity.Y = Math.Max(velocity.Y, TerminalFallingVelocity);

			// no velocity ? no collision!
			if (velocity == Vector2f.Zero)
				return false;

			var obstacles = game.Entities.Where(e => (e as Obstacle) != null).Select(o => o as Obstacle).ToArray();

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
