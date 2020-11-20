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
		private const int EntityTraceLength = 512;

		public void TrackEntity(ITrack entity)
		{
			if (entity == null)
				return;

			if (!TrackedEntities.ContainsKey(entity.TrackId()))
				TrackedEntities.Add(entity.TrackId(), new FixedQueue<TrackedData>(EntityTraceLength));

			TrackedEntities[entity.TrackId()].Add(entity.Track());
		}

		public void Render(AABB view, double interpolation, Entity[] entities)
		{
			if (Settings.Instance.GetDebug(DebugSetting.DrawAABB))
			{
				foreach (var entity in entities)
				{
					var position = entity.Position;
					if (entity is DynamicEntity dynamicEntity)
						position = dynamicEntity.InterpolatedPosition(interpolation);

					var entityColor = new Color4b((uint)entity.Name.GetHashCode()) { A = 0x4f };
					Renderer.Instance.RenderRectangle(view, entity.BoundingBox + position, Color4b.Transparent, entityColor);
				}

				var velocityColor = new Color4b(Color4b.Green) { A = 0xff };
				foreach (var entity in entities)
				{
					if (entity is DynamicEntity dynamicEntity && dynamicEntity.Velocity != Vector2f.Zero)
						Renderer.Instance.RenderRectangle(view,
							dynamicEntity.BoundingBox + dynamicEntity.Position + dynamicEntity.Velocity, velocityColor, Color4b.Transparent);
				}
			}

			if (Settings.Instance.GetDebug(DebugSetting.DrawHistory))
			{
				foreach (var trackedEntity in TrackedEntities)
				{
					if (trackedEntity.Value.Queue.Count < 2)
						continue;

					//var entityColor = new Color4b((uint)trackedEntity.Key.GetHashCode()) { A = 0xff };
					var values = trackedEntity.Value.Queue.ToArray();

					var positionColor = Color4b.Blue;
					for (var i = 1; i < values.Length; i++)
						Renderer.Instance.RenderLine(view, values[i - 1].Position, values[i].Position, positionColor);

					var velocityColor = Color4b.Green;
					for (var i = 0; i < values.Length; i++)
					{
						if (values[i].Velocity != Vector2f.Zero)
							Renderer.Instance.RenderLine(view, values[i].Position, values[i].Position + values[i].Velocity, velocityColor);
					}
				}
			}
		}

		internal void Clear()
		{
			TrackedEntities.Clear();
		}
	}
}
