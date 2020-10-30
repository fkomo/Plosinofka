using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka.Interfaces
{
	public struct TrackedData
	{
		public Vector2f Position;
		public Vector2f Velocity;
	}

	interface ITrackable
	{
		string TrackId();
		TrackedData Track();
	}
}
