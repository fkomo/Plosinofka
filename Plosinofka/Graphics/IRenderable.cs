
using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka.Interfaces
{
	interface IRenderable
	{
		void Render(AABB view, double interpolation);
	}
}
