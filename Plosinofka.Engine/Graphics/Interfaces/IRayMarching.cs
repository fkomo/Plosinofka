using Ujeby.Plosinofka.Engine.Common;

namespace Ujeby.Plosinofka.Engine.Graphics
{
	interface IRayMarching
	{
		double RayMarch(Vector2f origin, Vector2f direction, out Vector2f normal);
	}
}
