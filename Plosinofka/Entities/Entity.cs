using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	public abstract class Entity
	{
		public string Name { get; protected set; } = Guid.NewGuid().ToString("N");
		public override string ToString() => Name;

		protected AABB boundingBox;
		public AABB BoundingBox { get { return boundingBox; } protected set { boundingBox = value; } }

		protected Vector2f position;
		/// <summary>bottom left</summary>
		public Vector2f Position 
		{ 
			get { return position; } 
			set
			{
				position = value;
				boundingBox = new AABB(position, position + boundingBox.Size);
			}
		}
		public Vector2f Size => boundingBox.Size;
		public Vector2f Center => boundingBox.Center;

		public abstract void Update(IRayCasting environment);
	}
}
