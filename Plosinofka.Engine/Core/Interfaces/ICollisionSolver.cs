
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Entities;

namespace Ujeby.Plosinofka.Engine.Core
{
	public interface ICollisionSolver
	{
		/// <summary>
		/// solve all collisions in entity's path (position + velocity)
		/// returns new solved position and its remaining velocity 
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="position">solved position</param>
		/// <param name="velocity">remaining velocity</param>
		/// <returns>true if any collisions occured</returns>
		bool Solve(DynamicEntity entity, out Vector2f position, out Vector2f velocity);
	}
}
