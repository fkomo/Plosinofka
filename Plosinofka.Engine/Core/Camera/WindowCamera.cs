using System;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Entities;
using Ujeby.Plosinofka.Engine.Graphics;

namespace Ujeby.Plosinofka.Engine.Core
{
	/// <summary>
	/// player position is always inside camera window
	/// also snapped to world-edge
	/// </summary>
	public class WindowCamera : Camera, IRender
	{
		private AABB Window;

		public bool PlatformSnapping { get; set; } = true;

		public WindowCamera(Vector2i viewSize, AABB window) : base(viewSize)
		{
			Window = window;
		}

		public void Render(AABB view, double interpolation)
		{
			var color = new Color4b(Color4b.White) { A = 0x7f };

			// vertical center
			if (PlatformSnapping)
			{
				Renderer.Instance.RenderLineOverlay(view,
					new Vector2f(0, view.Size.Y / 2).Round(), new Vector2f(view.Size.X, view.Size.Y / 2).Round(),
					color);
			}

			// window
			Renderer.Instance.RenderLineOverlay(view,
				Window.Min.Round(), new Vector2f(Window.Right, Window.Bottom).Round(),
				color);
			Renderer.Instance.RenderLineOverlay(view,
				Window.Min.Round(), new Vector2f(Window.Left, Window.Top).Round(),
				color);
			Renderer.Instance.RenderLineOverlay(view,
				new Vector2f(Window.Left, Window.Top).Round(), Window.Max.Round(),
				color);
			Renderer.Instance.RenderLineOverlay(view,
				new Vector2f(Window.Right, Window.Bottom).Round(), Window.Max.Round(),
				color);
		}

		/// <summary>frame duration for smooth movement [ms]</summary>
		private const int UpdateDuration = 20;

		/// <summary>time of last smooth update frame [ms]</summary>
		private double LastUpdate;

		public override void Update(Player player, AABB edge)
		{
			BeforeUpdate();

			if (PlatformSnapping && player.ObstacleAt(Side.Down))
			{
				// smooth change to new height
				var newHeight = (int)player.Center.Y - Size.Y / 2;
				if (Math.Abs(newHeight - Origin.Y) > 1 && (Game.GetElapsed() - LastUpdate > UpdateDuration))
				{
					Origin.Y += (newHeight - Origin.Y) / 2;
					LastUpdate = Game.GetElapsed();
				}
			}

			var playerInCameraSpace = player.Center - Origin;
			if (!Window.Inside(playerInCameraSpace))
			{
				// move window to contain player
				if (playerInCameraSpace.X > Window.Right)
					Origin.X += (int)(playerInCameraSpace.X - Window.Right);
				else if (playerInCameraSpace.X < Window.Left)
					Origin.X += (int)(playerInCameraSpace.X - Window.Left);

				if (playerInCameraSpace.Y > Window.Top)
					Origin.Y += (int)(playerInCameraSpace.Y - Window.Top);
				else if (playerInCameraSpace.Y < Window.Bottom)
					Origin.Y += (int)(playerInCameraSpace.Y - Window.Bottom);
			}

			// edge snap
			Origin.X = Math.Min((int)edge.Max.X - Size.X, Math.Max((int)edge.Min.X, Origin.X));
			Origin.Y = Math.Min((int)edge.Max.Y - Size.Y, Math.Max((int)edge.Min.Y, Origin.Y));
		}
	}
}
