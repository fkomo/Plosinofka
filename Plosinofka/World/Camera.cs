using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	/// <summary>
	/// camera is a window to the world
	/// </summary>
	public class Camera
	{
		/// <summary>bottomLeft</summary>
		public Vector2i Position;
		public Vector2i View { get; private set; }

		private Vector2i PositionBeforeUpdate;
		private Vector2i ViewBeforeUpdate;

		public Camera(Vector2i view, Vector2i worldBorders, Entity target)
		{
			View = ViewBeforeUpdate = view;

			var position = PositionBeforeUpdate = target.Center - view / 2;
			position.X = Math.Min(worldBorders.X - View.X, Math.Max(0, Position.X));
			position.Y = Math.Min(worldBorders.Y - View.Y, Math.Max(0, Position.Y));

			Position = PositionBeforeUpdate = position;
		}

		public void Update(Entity target, Vector2i worldBorders)
		{
			ViewBeforeUpdate = View;
			PositionBeforeUpdate = Position;

			// camera is targeted at entity and its view wont go beyond world borders
			Position = target.Center - View / 2;
			Position.X = Math.Min(worldBorders.X - View.X, Math.Max(0, Position.X));
			Position.Y = Math.Min(worldBorders.Y - View.Y, Math.Max(0, Position.Y));
		}

		public Vector2i InterpolatedPosition(double interpolation)
		{
			return PositionBeforeUpdate + (Position - PositionBeforeUpdate) * interpolation;
		}

		internal Vector2i InterpolatedView(double interpolation)
		{
			return ViewBeforeUpdate + (View - ViewBeforeUpdate) * interpolation;
		}

		internal Vector2f RelateTo(Vector2f position, double interpolation)
		{
			return position - InterpolatedPosition(interpolation);
		}
	}
}
