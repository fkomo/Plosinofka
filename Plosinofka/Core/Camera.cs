using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	/// <summary>
	/// camera is a window to the world
	/// </summary>
	public abstract class Camera
	{
		protected Vector2i Origin;
		protected Vector2i Size;

		protected Vector2i OriginBeforeUpdate;
		protected Vector2i SizeBeforeUpdate;

		public AABB View { get { return new AABB(Origin, (Origin + Size)); } }
		protected AABB ViewBeforeUpdate
		{
			get
			{
				return new AABB(OriginBeforeUpdate, OriginBeforeUpdate + SizeBeforeUpdate);
			}
		}

		public Camera(Vector2i size)
		{
			Origin = OriginBeforeUpdate = Vector2i.Zero;
			Size = SizeBeforeUpdate = size;
		}

		protected void BeforeUpdate()
		{
			OriginBeforeUpdate = Origin;
			SizeBeforeUpdate = Size;
		}

		public abstract void Update(Entity target, Vector2i worldBorders);

		public AABB InterpolatedView(double interpolation)
		{
			var now = View;
			var before = ViewBeforeUpdate;

			var min = (before.Min + (now.Min - before.Min) * interpolation).Round();
			var max = (before.Max + (now.Max - before.Max) * interpolation).Round();

			return new AABB(min, max);
		}
	}
}
