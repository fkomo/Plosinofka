using System;
using Ujeby.Plosinofka.Engine.Common;

namespace Ujeby.Plosinofka.Engine.Entities
{
	public abstract class Entity
	{
		public string Name { get; protected set; } = Guid.NewGuid().ToString("N");
		public override string ToString() => $"{ this.GetType().Name }:{ Name }";

		public AABB BoundingBox { get; protected set; }

		/// <summary>bottom left</summary>
		public Vector2f Position;

		public Vector2f Size => BoundingBox.Size;
		public Vector2f Center => Position + BoundingBox.Center;

		public abstract void Update();
	}
}
