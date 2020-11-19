using Ujeby.Plosinofka.Engine.Common;

namespace Ujeby.Plosinofka.Engine.Entities
{
	public class Obstacle : DynamicEntity
	{
		public override bool Responsive => false;

		public override string ToString() => $"{ base.ToString() }; aabb:{ BoundingBox }";

		public Obstacle(AABB aabb)
		{
			BoundingBox = aabb;
		}
	}
}
