using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	public abstract class Entity
	{
		public string Name { get; protected set; } = Guid.NewGuid().ToString("N");
		public override string ToString() => Name;

		protected BoundingBox boundingBox;
		public BoundingBox BoundingBox => boundingBox;

		/// <summary>bottom left</summary>
		public Vector2f Position { get { return BoundingBox.Position; } set { boundingBox.Position = value; } }
		public Vector2f Size => BoundingBox.Size;
		public Vector2f Center => Position + Size / 2;

		public abstract void Update(IRayCasting environment);
	}
}
