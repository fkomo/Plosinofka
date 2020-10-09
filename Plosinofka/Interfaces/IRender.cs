using System;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka.Interfaces
{
	interface IRender
	{
		void Render(Camera camera, Entity beforeUpdate, double interpolation);
	}
}
