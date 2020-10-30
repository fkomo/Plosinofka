using System;
using System.Collections.Generic;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	public enum DecalsEnum
	{
		DustParticlesRight,

		Count
	}

	public static class Decals
	{
		public static Dictionary<DecalsEnum, Guid> Library { get; private set; } = 
			new Dictionary<DecalsEnum, Guid>();

		static Decals()
		{
			// TODO maybe move to explicitly called Load method

			Library.Add(DecalsEnum.DustParticlesRight, 
				ResourceCache.LoadAnimationSprite($".\\Content\\Effects\\dust-right.png").Id);
		}
	}

	public class Decal : Entity, IRenderable, IDestroyable
	{
		protected double LastFrameChange;
		/// <summary>desired delay betwen animation frames [ms]</summary>
		protected const double AnimationFrameDelay = 100;

		private Guid ResourceId;
		private int AnimationFrame = 0;
		private readonly int FrameCount;

		public Decal(Guid resourceId, Vector2f position)
		{
			Position = position;
			ResourceId = resourceId;

			var sprite = ResourceCache.Get<AnimationSprite>(ResourceId);
			FrameCount = sprite.Frames;
		}

		public void Render(Camera camera, double interpolation)
		{
			if (AnimationFrame < FrameCount)
				Renderer.Instance.RenderSpriteFrame(camera, interpolation,
					ResourceCache.Get<AnimationSprite>(ResourceId), AnimationFrame, Position);
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
