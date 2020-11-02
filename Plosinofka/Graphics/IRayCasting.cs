using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka.Interfaces
{
	public interface IRayCasting
	{
		double Trace(AABB box, Vector2f direction, out Vector2f normal);
		double Trace(Vector2f origin, Vector2f direction, out Vector2f normal);
		bool Intersect(Ray ray, double from = 0, double to = double.PositiveInfinity);
	}
}
