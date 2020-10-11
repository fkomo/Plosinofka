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
			Entities.Add(new Player("Jebko") { Position = new Vector2f(128, 128) });

			// make camera view smaller then window size for more pixelated look!
			Camera = new Camera(Renderer.Instance.WindowSize / 4, GetPlayerEntity());
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
			var solved = false;
			position = entity.Position;
			velocity = entity.Velocity;

			if (position.Y < entity.Size.Y)
			{
				position.Y = entity.Size.Y;
				velocity.Y = 0;
				solved = true;
			}

			if (position.Y > CurrentLevel.Size.Y)
			{
				position.Y = CurrentLevel.Size.Y;
				velocity.Y = 0;
				solved = true;
			}

			if (position.X < 0)
			{
				position.X = 0;
				velocity.X = 0;
				solved = true;
			}

			if (position.X > CurrentLevel.Size.X - entity.Size.X)
			{
				position.X = CurrentLevel.Size.X - entity.Size.X;
				velocity.X = 0;
				solved = true;
			}

			return solved;

			//// no movement, no collision
			//if (velocity.X == 0 && velocity.Y == 0)
			//	return false;

			//// no collision map, nothing to collide with
			//var ws = ResourceCache.Get<Sprite>(CurrentLevel.CollisionResourceId);
			//if (ws == null || ws.Data == null)
			//	return false;

			//position.X = Math.Max(0, Math.Min(ws.Size.X - entity.Size.X, position.X));
			//position.Y = Math.Max(entity.Size.Y, Math.Min(ws.Size.Y, position.Y));

			//Log.Add($"position: { position } -> velocity: { velocity }");

			//// starting/ending index of entity sprite in world map
			//var wsStart = (int)((ws.Size.Y - position.Y) * ws.Size.X + position.X) * 4;
			//var wsEnd = wsStart + ((entity.Size.Y - 1) * ws.Size.X + (entity.Size.X - 1)) * 4;

			//for (var y = 0; y < entity.Size.Y; y++)
			//	for (var x = 0; x < entity.Size.X; x++)
			//	{
			//		// if velocity is solved
			//		if (velocity.X == 0 && velocity.Y == 0)
			//			return true;

			//		var offset = (y * ws.Size.X + x) * 4;
			//		if (ws.Data[wsStart + offset] == 255 && ws.Data[wsStart + offset + 3] == 255)
			//		{
			//			if (velocity.Y < 0)
			//			{
			//				Log.Add($"{ entity } collision y:{ entity.Size.Y - y }");

			//				position.Y += entity.Size.Y - y;
			//				velocity.Y = 0;
			//			}
			//			if (velocity.X > 0 && y < entity.Size.Y / 2)
			//			{
			//				Log.Add($"{ entity } collision x:-{ entity.Size.X - x }");

			//				position.X -= entity.Size.X - x;
			//				velocity.X = 0;
			//			}
			//		}

			//		if (ws.Data[wsEnd - offset] == 255 && ws.Data[wsEnd - offset + 3] == 255)
			//		{
			//			if (velocity.Y > 0)
			//			{
			//				Log.Add($"{ entity } collision y:-{ y + 1 }");

			//				position.Y -= y + 1;
			//				velocity.Y = 0;
			//			}
			//			if (velocity.X < 0 && y > entity.Size.Y / 2)
			//			{
			//				Log.Add($"{ entity } collision x:{ entity.Size.X - x }");

			//				position.X += entity.Size.X - x;
			//				velocity.X = 0;
			//			}
			//		}
			//	}

			//// if position was changed, collision was solved
			//return entity.Position != position;
		}

		public override void Destroy()
		{
			
		}
	}
}
