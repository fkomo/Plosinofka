using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	public abstract class Entity
	{
		public string Name { get; protected set; } = Guid.NewGuid().ToString("N");
		public override string ToString() => Name;

		public AABB BoundingBox { get; protected set; }

		/// <summary>bottom left</summary>
		public Vector2f Position;

		public Vector2f Size => BoundingBox.Size;
		public Vector2f Center => Position + BoundingBox.Center;

		public abstract void Update(IRayCasting environment);
	}
}
