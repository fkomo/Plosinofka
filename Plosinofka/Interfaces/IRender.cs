using System;
using System.Collections.Generic;
using System.Text;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka.Interfaces
{
	interface IRender
	{
		void Render(IntPtr renderer, Entity beforeUpdate, double interpolation);
	}
}
