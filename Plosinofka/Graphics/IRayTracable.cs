using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka.Interfaces
{
	public interface IRayTracable
	{
		double Trace(Vector2f origin, Vector2f direction, out Vector2f normal);
	}
}