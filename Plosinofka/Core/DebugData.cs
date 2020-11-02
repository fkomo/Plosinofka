using System.Collections.Generic;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Core
{
	class DebugData
	{
		/// <summary>
		/// Entity.Name vs its TrackedData
		/// </summary>
		protected Dictionary<string, FixedQueue<TrackedData>> TrackedEntities =
			new Dictionary<string, FixedQueue<TrackedData>>();

		/// <summary>
		/// number of past records of entity properties (position, ...)
		/// </summary>
		private const int EntityTraceLength = 256;

		public void TrackEntity(ITrackable entity)
		{
			if (entity == null)
				return;

			if (!TrackedEntities.ContainsKey(entity.TrackId()))
				TrackedEntities.Add(entity.TrackId(), new FixedQueue<TrackedData>(EntityTraceLength));

			TrackedEntities[entity.TrackId()].Add(entity.Track());
		}

		public void Render(Camera camera, double interpolation, Entity[] entities, AABB[] obstacles)
		{
			var view = camera.InterpolatedView(interpolation);

			if (Settings.Current.GetDebug(DebugSetting.DrawVectors))
			{
				foreach (var entity in entities)
				{
					if (entity is DynamicEntity dynamicEntity)
					{
						var center = dynamicEntity.InterpolatedPosition(interpolation) +
							dynamicEntity.BoundingBox.Min + dynamicEntity.BoundingBox.Size * 0.5;

						Renderer.Instance.RenderLine(view, center, center + dynamicEntity.Velocity, Color4b.Green);
					}
				}
			}

			if (Settings.Current.GetDebug(DebugSetting.DrawAABB))
			{
				var color = new Color4b(0xff, 0x00, 0x00, 0xaf);
				foreach (var aabb in obstacles)
					Renderer.Instance.RenderRectangle(view, aabb, color);

				foreach (var entity in entities)
				{
					var position = entity.Position;
					if (entity is DynamicEntity dynamicEntity)
						position = dynamicEntity.InterpolatedPosition(interpolation);

					var entityColor = new Color4b((uint)entity.Name.GetHashCode()) { A = 0xff };
					Renderer.Instance.RenderRectangle(view, entity.BoundingBox + position, entityColor);
				}
			}

			if (Settings.Current.GetDebug(DebugSetting.MovementHistory))
			{
				foreach (var trackedEntity in TrackedEntities)
				{
					if (trackedEntity.Value.Queue.Count < 2)
						continue;

					var entityColor = new Color4b((uint)trackedEntity.Key.GetHashCode()) { A = 0xff };

					// positions
					var values = trackedEntity.Value.Queue.ToArray();
					for (var i = 1; i < values.Length; i++)
						Renderer.Instance.RenderLine(view, values[i - 1].Position, values[i].Position, entityColor);
				}
			}
		}

		internal void Clear()
		{
			TrackedEntities.Clear();
		}
	}
}
