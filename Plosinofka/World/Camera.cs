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
		public Vector2f TopLeft { get; private set; }
		public Vector2i View { get; private set; }

		public Vector2f TopLeftBeforeUpdate { get; private set; }
		public Vector2i ViewBeforeUpdate { get; private set; }

		public Camera(Vector2i view, Entity target)
		{
			View = ViewBeforeUpdate = view;

			var targetCenter = target.Center;
			TopLeft = TopLeftBeforeUpdate = new Vector2f(targetCenter.X - view.X / 2, targetCenter.Y + view.Y / 2);
		}

		public void Update(Entity target, Vector2i borders)
		{
			ViewBeforeUpdate = View;
			TopLeftBeforeUpdate = TopLeft;

			// camera is targeted at entity and its view wont go beyond world borders
			var targetCenter = target.Center;
			TopLeft = new Vector2f(
				Math.Min(borders.X - View.X, Math.Max(0, targetCenter.X - View.X / 2)),
				Math.Min(borders.Y, Math.Max(View.Y, targetCenter.Y + View.Y / 2)));
		}

		public Vector2f GetPosition(double interpolation)
		{
			return TopLeftBeforeUpdate + (TopLeft - TopLeftBeforeUpdate) * interpolation;
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
