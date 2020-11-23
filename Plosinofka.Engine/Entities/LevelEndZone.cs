using Ujeby.Plosinofka.Engine.Common;

namespace Ujeby.Plosinofka.Engine.Entities
{
	public class LevelEndZone : Entity
	{
		public override string ToString() => $"{ base.ToString() }; aabb:{ BoundingBox }";

		public override void Update()
		{
		}

		public LevelEndZone(AABB aabb)
		{
			BoundingBox = aabb;
		}

		public bool VsEntity(Entity entity)
		{
			return (entity.BoundingBox + entity.Position).Overlap(BoundingBox + Position);
		}
	}
}
