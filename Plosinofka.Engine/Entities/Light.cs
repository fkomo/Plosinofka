using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Graphics;

namespace Ujeby.Plosinofka.Engine.Entities
{
	public class Light : DynamicEntity
	{
		public Color4f Color { get; private set; }
		public double Intensity { get; private set; }

		public override bool Responsive => false;

		public Light(Color4f color, double intensity)
		{
			Color = color;
			Intensity = intensity;
		}

		public override void Update()
		{
			// save state before update
			PreviousPosition = Position;
			PreviousVelocity = Velocity;
		}
	}
}
