using System;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Entities;

namespace Ujeby.Plosinofka.Engine.Core
{
	public class PlayerCamera : Camera
	{
		public PlayerCamera(Vector2i viewSize) : base(viewSize)
		{
		}

		/// <summary>
		/// camera is targeted at entity and view wont go beyond world borders
		/// </summary>
		/// <param name="target"></param>
		/// <param name="worldBorders"></param>
		public override void Update(Entity target, Vector2i worldBorders)
		{
			base.BeforeUpdate();

			Origin = target.Center.Round() - Size / 2;
			Origin.X = Math.Min(worldBorders.X - Size.X, Math.Max(0, Origin.X));
			Origin.Y = Math.Min(worldBorders.Y - Size.Y, Math.Max(0, Origin.Y));
		}
	}
}
