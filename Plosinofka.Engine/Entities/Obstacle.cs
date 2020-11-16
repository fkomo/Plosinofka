using Ujeby.Plosinofka.Engine.Common;

namespace Ujeby.Plosinofka.Engine.Entities
{
	public class Obstacle : DynamicEntity
	{
		public override bool Responsive => false; 

		public Obstacle(AABB aabb)
		{
			BoundingBox = aabb;
		}
	}
}
