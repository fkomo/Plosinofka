using Ujeby.Plosinofka.Engine.Common;

namespace Ujeby.Plosinofka.Engine.Entities
{
	public struct TrackedData
	{
		public Vector2f Position;
		public Vector2f Velocity;
	}

	public interface ITrack
	{
		string TrackId();
		TrackedData Track();
	}
}
