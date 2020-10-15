using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka.Interfaces
{
	interface IRayMarching
	{
		double RayMarch(Vector2f origin, Vector2f direction, out Vector2f normal);
	}
}
