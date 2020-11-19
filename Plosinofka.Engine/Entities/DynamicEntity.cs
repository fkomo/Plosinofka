using Ujeby.Plosinofka.Engine.Common;

namespace Ujeby.Plosinofka.Engine.Entities
{
	public abstract class DynamicEntity : Entity
	{
		public Vector2f Velocity;

		public Vector2f PreviousVelocity { get; protected set; }
		public Vector2f PreviousPosition { get; protected set; }

		/// <summary>if true, entity is oblivious to surrounding entities</summary>
		public virtual bool Responsive { get; } = true;

		public override string ToString() => $"{ base.ToString() }; pos:{ Position }; vel:{ Velocity }";

		public override void Update()
		{
			// save state before update
			PreviousPosition = Position;
			PreviousVelocity = Velocity;
		}

		public Vector2f InterpolatedPosition(double interpolation) 
			=> PreviousPosition + (Position - PreviousPosition) * interpolation;
	}
}
