using Ujeby.Plosinofka.Engine.Common;

namespace Ujeby.Plosinofka.Engine.Graphics
{
	public abstract class TextLine
	{
	}

	public class EmptyLine : TextLine
	{
	}

	public class Text : TextLine
	{
		public string Value;
		public Color4b Color = Color4b.White;
	}

	public class Font
	{
		public string SpriteId;
		public string DataSpriteId;

		/// <summary></summary>
		public Vector2i CharSize;

		/// <summary></summary>
		public Vector2i Spacing;

		public AABB[] CharBoxes;
	}
}
