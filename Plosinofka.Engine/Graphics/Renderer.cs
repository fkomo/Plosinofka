using System;
using Ujeby.Plosinofka.Engine.Common;
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
		/// <returns></returns>
		public abstract Font GetCurrentFont();

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
		/// render filled rectangle
		/// </summary>
		/// <param name="camera"></param>
		/// <param name="rectangle"></param>
		/// <param name="color"></param>
		public abstract void RenderRectangle(AABB view, AABB rectangle, Color4b borderColor, Color4b fillColor);

		/// <summary>
		/// render colored line between point a and b in world space
		/// </summary>
		/// <param name="view"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="color"></param>
		public abstract void RenderLine(AABB view, Vector2f a, Vector2f b, Color4b color);

		/// <summary>
		/// render colored line between point a and b in screen space
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="color"></param>
		public abstract void RenderLineOverlay(AABB view, Vector2i a, Vector2i b, Color4b color);

		/// <summary>
		/// render sprite frame to screen
		/// </summary>
		/// <param name="view"></param>
		/// <param name="sprite">width == height, sprite is considered as animation strip</param>
		/// <param name="spritePosition">position in world space (bottomLeft)</param>
		public abstract void RenderSpriteFrame(AABB view, Sprite sprite, Vector2f spritePosition, int frame);

		/// <summary>
		/// render sprite to screen
		/// </summary>
		/// <param name="view"></param>
		/// <param name="sprite"></param>
		/// <param name="spritePosition">position in world space (bottomLeft)</param>
		public abstract void RenderSprite(AABB view, Sprite sprite, Vector2f spritePosition);

		/// <summary>
		/// render text lines in screen space
		/// </summary>
		/// <param name="view"></param>
		/// <param name="position"></param>
		/// <param name="lines"></param>
		/// <param name="color"></param>
		/// <param name="fontSize"></param>
		public abstract void RenderTextLinesOverlay(AABB view, Vector2i position, TextLine[] lines, Color4b color,
			double fontSize = 1);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lines"></param>
		/// <param name="font"></param>
		/// <returns></returns>
		public abstract Vector2i GetTextSize(TextLine[] lines, Font font = null);

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
