﻿using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka.Interfaces
{
	interface IRayCaster
	{
		double Intersect(BoundingBox box, Vector2f direction, out Vector2f normal);
		double Intersect(Vector2f origin, Vector2f direction, out Vector2f normal);
	}
}
