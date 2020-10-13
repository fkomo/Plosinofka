using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	abstract class Entity
	{
		public Guid Id { get; private set; } = Guid.NewGuid();
		public string Name { get; private set; }

		public BoundingBox BoundingBox;
		public Vector2f Velocity = Simulation.Gravity; // add gravity to created entity

		/// <summary>bottom left</summary>
		public Vector2f Position { get { return BoundingBox.Position; } set { BoundingBox.Position = value; } }
		public Vector2i Size => BoundingBox.Size;
		public Vector2f Center => Position + Size / 2;

		public override string ToString() => $"{ Id }-{ Name }";

		public abstract void Update(ICollisionSolver collisionSolver);

		protected Entity BeforeUpdate;

		protected Entity(string name) => Name = name;
	}
}
