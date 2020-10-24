﻿using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka.Entities
{
	public abstract class DynamicEntity : Entity
	{
		public Vector2f Velocity;

		protected Vector2f PreviousVelocity;
		protected Vector2f PreviousPosition;

		public Vector2f InterpolatedPosition(double interpolation) 
			=> PreviousPosition + (Position - PreviousPosition) * interpolation;
	}
}