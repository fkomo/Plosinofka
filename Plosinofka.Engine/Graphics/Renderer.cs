﻿using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Entities;

namespace Ujeby.Plosinofka.Engine.Graphics
{
	public abstract class Renderer
	{
		public static Renderer Instance { get; internal set; }

		/// <summary>max number of skippable frames to render</summary>
		public abstract int MaxFrameSkip { get; protected set; }
		public abstract double LastFrameDuration { get; protected set; }
		public abstract double LastShadingDuration { get; protected set; }
		public abstract long FramesRendered { get; protected set; }

		public abstract Vector2i CurrentWindowSize { get; }

		public abstract void Initialize();
		public abstract void Destroy();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="title"></param>
		public abstract void SetWindowTitle(string title);

		/// <summary>
		/// render simulation
		/// </summary>
		/// <param name="simulation"></param>
		/// <param name="interpolation"></param>
		public abstract void Render(Simulation simulation, double interpolation);

		/// <summary>
		/// render color filled aabb
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="rectangle"></param>
		/// <param name="color"></param>
		public abstract void RenderRectangle(AABB view, AABB rectangle, Color4b color);

		/// <summary>
		/// render colored line between point a and b
		/// </summary>
		/// <param name="view"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="color"></param>
		public abstract void RenderLine(AABB view, Vector2f a, Vector2f b, Color4b color);

		/// <summary>
		/// render sprite to screen
		/// </summary>
		/// <param name="view"></param>
		/// <param name="sprite">width == height, if not sprite is considered as animation strip</param>
		/// <param name="spritePosition">sprite position in world (bottomLeft)</param>
		public abstract void RenderSprite(AABB view, Sprite sprite, Vector2f spritePosition, int frame = 0);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="position">topLeft corner (increasing from top to bottom)</param>
		/// <param name="text"></param>
		public abstract void RenderText(AABB view, Vector2i position, string text);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="layer"></param>
		/// <param name="view"></param>
		public abstract void RenderLayer(AABB view, Layer layer);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="view"></param>
		/// <param name="layer"></param>
		/// <param name="lights"></param>
		/// <param name="obstacles"></param>
		public abstract void RenderLayer(AABB view, Layer layer, Light[] lights, AABB[] obstacles);
	}
}