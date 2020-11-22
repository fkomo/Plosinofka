using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Graphics;

namespace Ujeby.Plosinofka.Engine.Entities
{
	public class Decal : Entity, IRender, IDestroyable
	{
		protected double LastFrameChange;
		/// <summary>desired delay betwen animation frames [ms]</summary>
		protected readonly double AnimationFrameDelay;

		private readonly Sprite sprite;
		private readonly int FrameCount;
		private int AnimationFrame = 0;

		/// <summary>
		/// creates decal, position is centerd because i dont know the size of decal
		/// </summary>
		/// <param name="resourceId"></param>
		/// <param name="position">center of decal sprite</param>
		public Decal(Sprite sprite, Vector2f position, double frameDelay = 100)
		{
			this.sprite = sprite;
			AnimationFrameDelay = frameDelay;

			var frameSize = new Vector2i(sprite.Size.Y, sprite.Size.Y);
			Position = position - frameSize * 0.5;
			FrameCount = sprite.Size.X / frameSize.Y;
		}

		public void Render(AABB view, double interpolation)
		{
			if (AnimationFrame < FrameCount)
				Renderer.Instance.RenderSpriteFrame(view, sprite, Position, AnimationFrame);
		}

		public override void Update()
		{
			var current = Core.GameLoop.GetElapsed();
			if (current - LastFrameChange > AnimationFrameDelay)
			{
				AnimationFrame++;
				LastFrameChange = current;
			}
		}

		public bool Obsolete()
		{
			return AnimationFrame > FrameCount;
		}
	}
}
