using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka.Interfaces
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
		bool Solve(Entity entity, out Vector2f position, out Vector2f velocity);
	}
}
