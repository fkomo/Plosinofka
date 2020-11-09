using System.Collections.Generic;
using System.Linq;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Entities;
using Ujeby.Plosinofka.Engine.Graphics;

namespace Ujeby.Plosinofka.Game
{
	internal class DebugData
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

		public void Render(AABB view, double interpolation, Entity[] entities, AABB[] obstacles)
		{
			if (Settings.Instance.GetDebug(DebugSetting.DrawAABB))
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

				foreach (var entity in entities)
				{
					if (entity is DynamicEntity dynamicEntity)
					{
						var entityColor = new Color4b(Color4b.Green) { A = 0x7f };
						Renderer.Instance.RenderRectangle(view,
							dynamicEntity.BoundingBox + dynamicEntity.Position + dynamicEntity.Velocity, entityColor);
					}
				}
			}

			if (Settings.Instance.GetDebug(DebugSetting.DrawHistory))
			{
				foreach (var trackedEntity in TrackedEntities)
				{
					if (trackedEntity.Value.Queue.Count < 2)
						continue;

					var entityColor = new Color4b((uint)trackedEntity.Key.GetHashCode()) { A = 0xff };

					// past positions
					var values = trackedEntity.Value.Queue.ToArray();

					//var entity = entities.SingleOrDefault(e => (e as ITrackable)?.TrackId() == trackedEntity.Key);
					//if (entity != null)
					//{
					//	var entityAabb = new AABB(Vector2f.Zero, entity.BoundingBox.Size);
					//	for (var i = 0; i < values.Length; i++)
					//	{
					//		entityColor.A = (byte)((double)i / values.Length * 255);
					//		Renderer.Instance.RenderRectangle(view,
					//			entityAabb + (values[i].Position - entity.BoundingBox.HalfSize), 
					//			entityColor);
					//	}
					//}

					entityColor.A = 0xff;
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
