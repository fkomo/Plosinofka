
using Ujeby.Plosinofka.Engine.Common;

namespace Ujeby.Plosinofka.Engine.Graphics
{
	public interface IRender
	{
		void Render(AABB view, double interpolation);
	}
}
