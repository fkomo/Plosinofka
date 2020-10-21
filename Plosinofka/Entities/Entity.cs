using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	public abstract class Entity
	{
		public string Name { get; protected set; } = Guid.NewGuid().ToString("N");
		public override string ToString() => Name;

		protected Vector2f position;
		protected BoundingBox boundingBox;

		public BoundingBox BoundingBox => boundingBox;

		/// <summary>bottom left</summary>
		public Vector2f Position 
		{ 
			get { return position; } 
			set
			{
				position = value;
				boundingBox = new BoundingBox(position, position + BoundingBox.Size);
			}
		}
		public Vector2f Size => BoundingBox.Max - BoundingBox.Min;
		public Vector2f Center => BoundingBox.Center;

		public abstract void Update(IRayCasting environment);
	}
}
