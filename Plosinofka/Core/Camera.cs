using System;
using System.Net.NetworkInformation;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	/// <summary>
	/// camera is a window to the world
	/// </summary>
	public class Camera
	{
		public AABB View { get; private set; }
		private AABB ViewBeforeUpdate;

		public Camera(Vector2i size, Vector2i worldBorders, Entity target)
		{
			ViewBeforeUpdate = View = new AABB(Vector2f.Zero, (Vector2f)size);
			
			Update(target, worldBorders);
		}

		public void Update(Entity target, Vector2i worldBorders)
		{
			ViewBeforeUpdate = View;

			// camera is targeted at entity and its view wont go beyond world borders
			var newMin = target.Center - View.Size * 0.5;
			newMin.X = Math.Min(worldBorders.X - View.Size.X, Math.Max(0, newMin.X));
			newMin.Y = Math.Min(worldBorders.Y - View.Size.Y, Math.Max(0, newMin.Y));

			View = new AABB(Vector2f.Zero, View.Size) + newMin;
		}

		public AABB InterpolatedView(double interpolation)
		{
			return new AABB(
				ViewBeforeUpdate.Min + (View.Min - ViewBeforeUpdate.Min) * interpolation,
				ViewBeforeUpdate.Max + (View.Max - ViewBeforeUpdate.Max) * interpolation);
		}
	}
}
