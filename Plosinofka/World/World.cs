using System.Linq;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka
{
	class World : Simulation, ICollisionSolver
	{
		private Level CurrentLevel;

		public World()
		{
			CurrentLevel = Level.Load("world1");
			Entities.Add(new Player("jebko") 
			{ 
				Position = new Vector2f(128, 128)
			});

			// make camera view smaller then window size for more pixelated look!
			Camera = new Camera(Renderer.Instance.WindowSize, GetPlayerEntity());
		}

		public override void Update()
		{
			var start = Game.GetElapsed();

			// update all entities
			foreach (var entity in Entities)
				entity.Update(this);

			// update camera with respect to world/level borders
			Camera.Update(GetPlayerEntity(), CurrentLevel.Size);

			LastUpdateDuration = Game.GetElapsed() - start;
		}

		public override void Render(double interpolation)
		{
			// render background
			RenderBackground(interpolation);

			// render renderable entities
			foreach (IRender entity in Entities.Where(e => e is IRender))
				entity.Render(Camera, interpolation);

			// render foreground
			RenderForeground(interpolation);
		}

		private void RenderBackground(double interpolation)
		{
			var background = ResourceCache.Get<Sprite>(CurrentLevel.Resources[(int)LevelResourceType.Background]);
			if (background == null)
				return;

			Renderer.Instance.RenderSprite(Camera, background, interpolation);
		}

		private void RenderForeground(double interpolation)
		{
			// TODO render world foreground
		}

		public bool Solve(Entity entity, out Vector2f position, out Vector2f velocity)
		{
			position = entity.Position;
			velocity = entity.Velocity;

			var collisionFound = false;
			while (true)
			{
				var distance = velocity.Length();
				var direction = velocity.Normalize();

				var t = CurrentLevel.Intersect(entity.BoundingBox, velocity.Normalize(), out Vector2f normal);

				if (normal == Vector2f.Up && t >= 0 && t < distance)
				{
					t -= 1;

					// obstacle in path
					collisionFound = true;
					// compute new position & velocity
					position += direction * t;

					velocity = Vector2f.Zero;
					if (direction.X > 0)
						velocity = Vector2f.Right * (distance - t);
					else if (direction.X < 0)
						velocity = Vector2f.Left * (distance - t);
				}
				else if (normal == Vector2f.Left && t >= 0 && t < distance + entity.Size.X)
				{
					// obstacle in path
					collisionFound = true;
					// compute new position & velocity
					position += (direction * t) - new Vector2f(entity.Size.X, 0);

					velocity = Vector2f.Zero;
					if (direction.Y > 0)
						velocity = Vector2f.Up * (distance - t);
					else if (direction.Y < 0)
						velocity = Vector2f.Down * (distance - t);
				}
				else
					break;

				if (velocity == Vector2f.Zero) // or distance < ~1 ?
					break;

				//if (t >= 0 && t < distance)
				//{
				//	velocity = Vector2f.Zero;

				//	if (normal == Vector2f.Up)
				//	{
				//		if (direction.X > 0)
				//			velocity = Vector2f.Right * (distance - t);
				//		else if (direction.X < 0)
				//			velocity = Vector2f.Left * (distance - t);
				//	}
				//	else if (normal == Vector2f.Down)
				//	{
				//		position.Y -= entity.Size.Y;
				//		if (direction.X > 0)
				//			velocity = Vector2f.Right * (distance - t);
				//		else if (direction.X < 0)
				//			velocity = Vector2f.Left * (distance - t);
				//	}
				//	else if (normal == Vector2f.Left)
				//	{
				//		position.X -= entity.Size.X;
				//		if (direction.Y > 0)
				//			velocity = Vector2f.Up * (distance - t);
				//		else if (direction.Y < 0)
				//			velocity = Vector2f.Down * (distance - t);
				//	}
				//	else if (normal == Vector2f.Right)
				//	{
				//		if (direction.Y > 0)
				//			velocity = Vector2f.Up * (distance - t);
				//		else if (direction.Y < 0)
				//			velocity = Vector2f.Down * (distance - t);
				//	}
			}

			return collisionFound;
		}

		public override void Destroy()
		{
			
		}
	}
}
