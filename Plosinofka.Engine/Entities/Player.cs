using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Graphics;

namespace Ujeby.Plosinofka.Engine.Entities
{
	public abstract class Player : DynamicEntity, IRender, IHandleInput
	{
		public abstract void Render(AABB view, double interpolation);

		public abstract void HandleButton(InputButton button, InputButtonState state);
	}
}
