using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	public enum DecalsEnum
	{
		DustParticlesRight,
		DustParticlesLeft,

		Count
	}

	public class Decal : Entity, IRenderable, IDestroyable
	{
		protected double LastFrameChange;
		/// <summary>desired delay betwen animation frames [ms]</summary>
		protected const double AnimationFrameDelay = 100;

		private readonly Sprite sprite;
		private readonly int FrameCount;
		private int AnimationFrame = 0;

		/// <summary>
		/// creates decal, position is centerd because i dont know the size of decal
		/// </summary>
		/// <param name="resourceId"></param>
		/// <param name="position">center of decal sprite</param>
		public Decal(DecalsEnum spriteId, Vector2f position)
		{
			sprite = SpriteCache.Get(spriteId.ToString());
			var frameSize = new Vector2i(sprite.Size.Y, sprite.Size.Y);
			Position = position - frameSize * 0.5;
			FrameCount = sprite.Size.X / frameSize.Y;
		}

		public void Render(Camera camera, double interpolation)
		{
			if (AnimationFrame < FrameCount)
			{
				var view = camera.InterpolatedView(interpolation);
				Renderer.Instance.RenderSprite(view, sprite, Position, frame:AnimationFrame);
			}
		}

		public override void Update(IRayCasting environment)
		{
			var current = Game.GetElapsed();
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
