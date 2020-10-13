using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	/// <summary>
	/// camera is a window to the world
	/// </summary>
	class Camera
	{
		/// <summary>bottomLeft</summary>
		public Vector2f Position { get; private set; }
		public Vector2i View { get; private set; }

		private Vector2f PositionBeforeUpdate;
		private Vector2i ViewBeforeUpdate;

		public Camera(Vector2i view, Entity target)
		{
			View = ViewBeforeUpdate = view;

			var targetCenter = target.Center;
			Position = PositionBeforeUpdate = targetCenter - view / 2;
		}

		public void Update(Entity target, Vector2i borders)
		{
			ViewBeforeUpdate = View;
			PositionBeforeUpdate = Position;

			// camera is targeted at entity and its view wont go beyond world borders
			Position = new Vector2f(
				Math.Min(borders.X - View.X, Math.Max(0, target.Center.X - View.X / 2)),
				Math.Min(borders.Y - View.Y, Math.Max(0, target.Center.Y - View.Y / 2)));
		}

		public Vector2f GetPosition(double interpolation)
		{
			return PositionBeforeUpdate + (Position - PositionBeforeUpdate) * interpolation;
		}

		internal Vector2f RelateTo(Vector2f position, double interpolation)
		{
			return position - GetPosition(interpolation);
		}

		internal Vector2i InterpolatedView(double interpolation)
		{
			return ViewBeforeUpdate + (View - ViewBeforeUpdate) * interpolation;
		}
	}
}
