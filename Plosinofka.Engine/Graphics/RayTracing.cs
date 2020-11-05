using System;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Entities;

namespace Ujeby.Plosinofka.Engine.Graphics
{
	public class RayTracing
	{
		public static Color4f DirectLight(Vector2i pixel, Light[] lights, AABB[] obstacles)
		{
			var rayOrigin = pixel;
			var result = Color4f.Black;

			var visibleLights = 0;
			foreach (var light in lights)
			{
				var lightDistance = (light.Position - rayOrigin).Length();
				var ray = new Ray(rayOrigin, light.Position - rayOrigin);

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

				visibleLights++;
				result += light.Color * (light.Intensity / lightDistance);
			}

			result.A = 1.0 - (double)visibleLights / lights.Length;
			
			return result;
		}

		public static Color4f AmbientOcclusion(Vector2i pixel, AABB[] obstacles, 
			double probeDistance = 128, int rayCount = 16)
		{
			var Rng = new Random();
			
			var rayOrigin = pixel;

			var occlusions = 0;
			for (var i = 0; i < rayCount; i++)
			{
				var ray = new Ray(rayOrigin, new Vector2f(Rng.NextDouble(), Rng.NextDouble()));

				var occluded = false;
				foreach (var obstacle in obstacles)
				{
					if (obstacle.Intersects(ray, to: probeDistance))
					{
						occluded = true;
						break;
					}
				}

				if (!occluded)
					occlusions++;
			}

			var ao = (double)occlusions / rayCount;
			return new Color4f(ao, ao, ao, 1);
		}
	}
}
