using System;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Graphics;

namespace Ujeby.Plosinofka.Engine.Entities
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

		public abstract void Update(IEnvironment environment);
	}
}
