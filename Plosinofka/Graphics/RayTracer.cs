using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Graphics
{
	public class RayTracer
	{
		public static Color4f Shade(Vector2i pixel, Light[] lights, AABB[] obstacles)
		{
			var origin = (Vector2f)pixel;
			var result = Color4f.Black;
			foreach (var light in lights)
			{
				var lightDistance = (light.Position - origin).Length();
				var ray = new Ray(origin, light.Position - origin);

				var occluded = false;
				foreach (var obstacle in obstacles)
				{
					if (obstacle.Intersects(ray, to: lightDistance))
					{
						occluded = true;
						break;
					}
				}
				if (occluded)
					continue;

				result += light.Color * (light.Intensity / (lightDistance));
			}

			return result;
		}
	}
}
