using System;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Entities;
using Ujeby.Plosinofka.Engine.Graphics;

namespace Ujeby.Plosinofka.Engine.Core
{
	/// <summary>
	/// camera is locked at given target position
	/// also snapped to world-edge
	/// </summary>
	public class PositionLockedCamera : Camera, IRender
	{
		public PositionLockedCamera(Vector2i viewSize) : base(viewSize)
		{
		}

		public void Render(AABB view, double interpolation)
		{
			var color = new Color4b(Color4b.White) { A = 0x7f };

			// center
			Renderer.Instance.RenderLineOverlay(view,
				new Vector2f(0, view.Size.Y / 2).Round(), new Vector2f(view.Size.X, view.Size.Y / 2).Round(),
				color);
			Renderer.Instance.RenderLineOverlay(view,
				new Vector2f(view.Size.X / 2, 0).Round(), new Vector2f(view.Size.X / 2, view.Size.Y).Round(),
				color);
		}

		public override void Update(Player player, AABB edge)
		{
			base.BeforeUpdate();

			Origin = player.Center.Round() - Size / 2;
			Origin.X = Math.Min((int)edge.Max.X - Size.X, Math.Max((int)edge.Min.X, Origin.X));
			Origin.Y = Math.Min((int)edge.Max.Y - Size.Y, Math.Max((int)edge.Min.Y, Origin.Y));
		}
	}
}
