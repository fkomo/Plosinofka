﻿using Ujeby.Plosinofka.Common;

namespace Ujeby.Plosinofka.Graphics
{
	public class Text
	{
		public string Value;
		public Vector2i Position;
		public Color4b Color;
	}

	public class Font
	{
		public string SpriteId;

		/// <summary></summary>
		public Vector2i CharSize;

		/// <summary></summary>
		public Vector2i Spacing;
	}
}
