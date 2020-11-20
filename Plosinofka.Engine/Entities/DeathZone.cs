using Ujeby.Plosinofka.Engine.Common;

namespace Ujeby.Plosinofka.Engine.Entities
{
	public class DeathZone : Entity
	{
		public override string ToString() => $"{ base.ToString() }; aabb:{ BoundingBox }";

		public override void Update()
		{
		}

		public DeathZone(AABB aabb)
		{
			BoundingBox = aabb;
		}

		public bool VsEntity(Entity entity)
		{
			return (entity.BoundingBox + entity.Position).Overlap(BoundingBox + Position);
		}
	}
}
