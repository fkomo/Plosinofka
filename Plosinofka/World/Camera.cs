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
			Position = target.Center - View / 2;
			Position.X = Math.Min(borders.X - View.X, Math.Max(0, Position.X));
			Position.Y = Math.Min(borders.Y - View.Y, Math.Max(0, Position.Y));
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
