
using System;
using System.Reflection.Metadata.Ecma335;

namespace Ujeby.Plosinofka.Common
{
	public struct Color4b
	{
		public byte R;
		public byte G;
		public byte B;
		public byte A;

		public static Color4b White = new Color4b(1, 1, 1, 1);
		public static Color4b Black = new Color4b(0, 0, 0, 1);

		public Color4b(byte r, byte g, byte b, byte a)
		{
			R = a;
			G = g;
			B = b;
			A = a;
		}

		public Color4b(Color4f c)
		{
			R = (byte)Math.Clamp(c.R * 255.0, 0.0, 255.0);
			G = (byte)Math.Clamp(c.G * 255.0, 0.0, 255.0);
			B = (byte)Math.Clamp(c.B * 255.0, 0.0, 255.0);
			A = (byte)Math.Clamp(c.A * 255.0, 0.0, 255.0);
		}

		public Color4b(uint value)
		{
			A = (byte)((value & 0xff000000) >> 24);
			B = (byte)((value & 0x00ff0000) >> 16);
			G = (byte)((value & 0x0000ff00) >> 8);
			R =  (byte)(value & 0x000000ff);
		}

		public uint AsUint =>
			((uint)R +
			((uint)G << 8) +
			((uint)B << 16) +
			((uint)A << 24));

		public override string ToString() => $"0x{ AsUint:x}";

		public static Color4b operator *(Color4b c1, Color4f c2)
		{
			return new Color4b
			{
				R = (byte)(c1.R * c2.R),
				G = (byte)(c1.G * c2.G),
				B = (byte)(c1.B * c2.B),
				A = (byte)(c1.A * c2.A),
			};
		}
	}

	public struct Color4f
	{
		public double R;
		public double G;
		public double B;
		public double A;

		public static Color4f White = new Color4f(1.0, 1.0, 1.0, 1.0);
		public static Color4f Black = new Color4f(0.0, 0.0, 0.0, 1.0);

		public Color4f(Color4f v) : this(v.R, v.G, v.B, v.A)
		{
		}

		public Color4f(double r, double g, double b) : this(r, g, b, 1)
		{
		}

		public Color4f(double r, double g, double b, double a)
		{
			R = r;
			G = g;
			B = b;
			A = a;
		}

		public Color4f(uint value)
		{
			A = ((value & 0xff000000) >> 24) / 255.0;
			B = ((value & 0x00ff0000) >> 16) / 255.0;
			G = ((value & 0x0000ff00) >> 8) / 255.0;
			R = ((value & 0x000000ff)) / 255.0;
		}

		public uint AsUint => 
			((uint)(Math.Clamp(R, 0, 1) * 255) +
			((uint)(Math.Clamp(G, 0, 1) * 255) << 8) +
			((uint)(Math.Clamp(B, 0, 1) * 255) << 16) +
			((uint)(Math.Clamp(A, 0, 1) * 255) << 24));

		public override string ToString() => $"0x{ AsUint:x}";

		public static Color4f operator *(Color4f c1, Color4f c2) => new Color4f(c1.R * c2.R, c1.G * c2.G, c1.B * c2.B);
		public static Color4f operator +(Color4f c1, Color4f c2) => new Color4f(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B);
		public static Color4f operator *(Color4f c, double k) => new Color4f(c.R * k, c.G * k, c.B * k);

		public Color4f Clamp() => new Color4f(Math.Clamp(R, 0.0, 1.0), Math.Clamp(G, 0.0, 1.0), Math.Clamp(B, 0.0, 1.0));
		public Color4f GammaCorrection() => new Color4f(Math.Pow(R, 0.4545), Math.Pow(G, 0.4545), Math.Pow(B, 0.4545));
	}
}
