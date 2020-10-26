using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka.Entities
{
	public abstract class DynamicEntity : Entity
	{
		public Vector2f Velocity;

		public Vector2f PreviousVelocity { get; protected set; }
		public Vector2f PreviousPosition { get; protected set; }

		public Vector2f InterpolatedPosition(double interpolation) 
			=> PreviousPosition + (Position - PreviousPosition) * interpolation;
	}
}
