using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka
{
	public class RoomCamera : Camera
	{
		public RoomCamera(Vector2i viewSize) : base(viewSize)
		{
		}

		/// <summary>
		/// camera is fixed to current room the player is in
		/// </summary>
		/// <param name="target"></param>
		/// <param name="worldBorders"></param>
		public override void Update(Entity target, Vector2i worldBorders)
		{
			base.BeforeUpdate();

			Origin = (target.Center / Size).Trunc() * Size;
			if (Origin != OriginBeforeUpdate)
			{
				// room changed
			}

			// set origin before to the same state, so interpolation would always result in Origin
			OriginBeforeUpdate = Origin;
		}
	}
}
