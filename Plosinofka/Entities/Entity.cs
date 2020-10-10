using System;
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka.Entities
{
	abstract class Entity
	{
		/// <summary></summary>
		public Guid Id { get; protected set; } = Guid.NewGuid();
		/// <summary></summary>
		public string Name { get; protected set; }
		/// <summary>top left</summary>
		public Vector2f Position;
		/// <summary></summary>
		public Vector2f Velocity = Simulation.Gravity;
		/// <summary></summary>
		public Vector2f Center => Position + (Size / 2);
		/// <summary></summary>
		public abstract Vector2i Size { get; }

		public abstract void Update();

		public abstract Entity Copy();

		public override string ToString()
		{
			return $"{ Name } [{ Id }]";
		}

		protected Entity()
		{
		}

		protected Entity(string name) : this()
		{
			Name = name;
		}
	}
}
