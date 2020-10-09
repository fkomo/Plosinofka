using System;

namespace Ujeby.Plosinofka.Entities
{
	abstract class Entity
	{
		public Guid Id { get; protected set; } = Guid.NewGuid();

		public string Name { get; protected set; }

		/// <summary></summary>
		public Vector2f Position;

		/// <summary></summary>
		public Vector2f Velocity = Simulation.Gravity;

		public abstract void Update();

		public abstract Entity Copy();

		public override string ToString()
		{
			return $"{ Name } [{ Id }]";
		}
	}
}
