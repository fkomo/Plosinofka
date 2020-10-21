using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	public class Light : DynamicEntity
	{
		public Color4f Color { get; private set; }
		public double Intensity { get; private set; }

		public Light(Color4f color, double intensity)
		{
			Color = color;
			Intensity = intensity;
		}

		//public void Render(Camera camera, double interpolation)
		//{
		//	var interpolatedPosition = PreviousPosition + (Position - PreviousPosition) * interpolation;
		//	var bb = new BoundingBox(interpolatedPosition, BoundingBox.Size);
		//	Renderer.Instance.RenderRectangle(camera, bb, Color, interpolation);
		//}

		public override void Update(IRayCasting environment)
		{
			// save state before update
			PreviousPosition = Position;
			PreviousVelocity = Velocity;
		}
	}
}
