using System;
using System.Collections.Generic;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka.Interfaces
{
	interface ICollisionSolver
	{
		bool Solve(Entity entity, out Vector2f position, out Vector2f velocity);
	}
}
