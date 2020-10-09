using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace Ujeby.Plosinofka.Entities
{
	abstract class Entity
	{
		public Guid Id { get; protected set; } = Guid.NewGuid();

		public string Name { get; protected set; }

		/// <summary>
		/// Entity position (center)
		/// </summary>
		public Vector2f Position;

		public abstract void Update();

		public abstract Entity Copy();
	}
}
