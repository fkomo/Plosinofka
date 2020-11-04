using Ujeby.Plosinofka.Engine.Common;

namespace Ujeby.Plosinofka.Engine.Graphics
{
	public interface ISdf
	{
		double Distance(Vector2f p);

		AABB GetAABB();
	}
}