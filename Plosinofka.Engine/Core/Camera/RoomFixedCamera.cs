using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Entities;

namespace Ujeby.Plosinofka.Engine.Core
{
	/// <summary>
	/// camera is fixed to the room that player currently in.
	/// room is part of level that with size of camera viewport
	/// also snapped to world-edge
	/// </summary>
	public class RoomFixedCamera : Camera
	{
		public RoomFixedCamera(Vector2i viewSize) : base(viewSize)
		{
		}

		public override void Update(Player player, AABB edge)
		{
			base.BeforeUpdate();

			Origin = (player.Center / Size).Trunc() * Size;
			if (Origin != OriginBeforeUpdate)
			{
				// room changed
			}

			// set origin before to the same state, so interpolation would always result in Origin
			OriginBeforeUpdate = Origin;
		}
	}
}
