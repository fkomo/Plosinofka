using Ujeby.Plosinofka.Engine.Common;

namespace Ujeby.Plosinofka.Engine.Graphics
{
	public interface IRayTracing
	{
		double Trace(Vector2f origin, Vector2f direction, out Vector2f normal);
		bool Intersect(Ray ray, double from = 0, double to = double.PositiveInfinity);
	}
}