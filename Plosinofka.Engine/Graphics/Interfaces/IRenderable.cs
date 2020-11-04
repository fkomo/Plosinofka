
using Ujeby.Plosinofka.Engine.Common;

namespace Ujeby.Plosinofka.Engine.Graphics
{
	public interface IRenderable
	{
		void Render(AABB view, double interpolation);
	}
}
