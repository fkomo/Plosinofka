using Ujeby.Plosinofka.Engine.Common;

namespace Ujeby.Plosinofka.Engine.Graphics
{
	public struct Layer
	{
		public string ColorMapId;
		public string NormalMapId;
		public string DataMapId;
		public int Depth;
		public bool Parallax;
		public Vector2i Size;
	}
}
