using System.Linq;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka
{
	public class World : Simulation, ICollisionSolver
	{
		private Level CurrentLevel;

		public World()
		{
			CurrentLevel = Level.Load("world2");

			var player = new Player("jebko") { Position = new Vector2f(64, 128) };
			Entities.Add(player);

			Entities.Add(new Light(new Color4f(1.0, 1.0, 0.8), 64.0) { Position = new Vector2f(240, 220) });
			Entities.Add(new Light(new Color4f(1.0, 0.0, 0.0), 16.0) { Position = new Vector2f(80, 100) });

			// make camera view smaller then window size for more pixelated look!
			Camera = new Camera(Vector2i.FullHD / 4, GetPlayerEntity());
		}

		public World(Level level)
		{
			CurrentLevel = level;
		}

		public override void Update()
		{
			var start = Game.GetElapsed();

			// update all entities
			foreach (var entity in Entities)
				entity.Update(CurrentLevel);

			// solve collisions of dynamic entities
			foreach (DynamicEntity dynamicEntity in Entities.Where(e => e is DynamicEntity))
			{
				Solve(dynamicEntity, out Vector2f position, out Vector2f velocity);
				dynamicEntity.Position = position;
				dynamicEntity.Velocity = velocity;
			}

			// update camera with respect to world/level borders
			Camera.Update(GetPlayerEntity(), CurrentLevel.Size);

			LastUpdateDuration = Game.GetElapsed() - start;
		}

		public override void Render(double interpolation)
		{
			// render background
			RenderBackground(interpolation);

			// render renderable entities
			foreach (IRenderable entity in Entities.Where(e => e is IRenderable))
				entity.Render(Camera, interpolation);

			// render foreground
			RenderForeground(interpolation);
		}

		private void RenderBackground(double interpolation)
		{
			// TODO paralax scrolling, multiple background layers

			var background = ResourceCache.Get<Sprite>(CurrentLevel.Resources[(int)LevelResourceType.Background]);
			if (background == null)
				return;

			var data = ResourceCache.Get<Sprite>(CurrentLevel.Resources[(int)LevelResourceType.Data]);

			var player = GetPlayerEntity();
			var playerBb = player.BoundingBox;
			playerBb.Position = player.InterpolatedPosition(interpolation);

			Renderer.Instance.Render(Camera, background, data, interpolation,
				Entities.Where(e => e is Light).Select(e => e as Light),
				CurrentLevel.Colliders.Concat(new[] { playerBb }));
		}

		private void RenderForeground(double interpolation)
		{
			// TODO render world foreground
		}

		public bool Solve(DynamicEntity entity, out Vector2f position, out Vector2f velocity)
		{
			position = entity.Position;
			velocity = entity.Velocity;
			var entityBox = entity.BoundingBox;
			var remainingVelocity = velocity;

			// no velocity ? no collision!
			if (velocity == Vector2f.Zero)
				return false;

			var collisionFound = false;
			while (true)
			{
				var distance = velocity.Length();
				var direction = velocity.Normalize();

				var t = CurrentLevel.Intersect(entityBox, direction, out Vector2f normal);
				if (t <= distance)
				{
					collisionFound = true;

					var hitPosition = position + direction * t;
					
					// set new position & velocity
					velocity = (position + velocity) 
						+ normal * Vector2f.Dot(direction.Inv(), normal) * (distance - t) 
						- hitPosition;
					position = hitPosition;
					entityBox.Position = position;

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

			Log.Add($"Solved({ entity }; position={ entity.Position }; velocity={ entity.Velocity }): position={ position }; velocity={ velocity }");

			return collisionFound;
		}

		public override void Destroy()
		{
			
		}
	}
}
