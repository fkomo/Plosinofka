using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka.Interfaces
{
	public interface IRayTracable
	{
		double Trace(Vector2f origin, Vector2f direction, out Vector2f normal);
		bool Intersects(Ray ray, double from = 0, double to = double.PositiveInfinity);
	}
}