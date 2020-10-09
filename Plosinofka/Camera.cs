using System;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	/// <summary>
	/// camera is a window to the world
	/// </summary>
	class Camera
	{
		public Vector2f Position { get; private set; }
		public Vector2i Size { get; private set; }

		public Vector2f PositionBeforeUpdate { get; private set; }
		public Vector2i SizeBeforeUpdate { get; private set; }

		public Camera(Vector2i size, Vector2f position)
		{
			Size = SizeBeforeUpdate = size;
			Position = PositionBeforeUpdate = position;
		}

		public void Update(Entity targetEntity, World world)
		{
			SizeBeforeUpdate = Size;
			PositionBeforeUpdate = Position;

			// camera is targeted at entity position
			var x = Math.Min(world.CurrentLevel.Size.X - Size.X / 2, Math.Max(Size.X / 2, targetEntity.Position.X));
			var y = Math.Min(world.CurrentLevel.Size.Y - Size.Y / 2, Math.Max(Size.Y / 2, targetEntity.Position.Y));
			Position = new Vector2f(x, y);
		}

		public Vector2f GetPosition(double interpolation)
		{
			return PositionBeforeUpdate + (Position - PositionBeforeUpdate) * interpolation;
		}

		internal Vector2f RelateTo(Vector2f position, double interpolation)
		{
			return position - (GetPosition(interpolation) - Size / 2);
		}
	}
}
