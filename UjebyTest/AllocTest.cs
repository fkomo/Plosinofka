using SDL2;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using System.Threading.Tasks;
using Ujeby.Plosinofka;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;

namespace UjebyTest
{
	class AllocTest : TestBase
	{
		public AllocTest() : base()
		{
		}

		protected override void Init()
		{
			var n = 100000000;
			double result = 0;

			var start = Program.Elapsed();
			for (var i = 0; i < n; i++)
			{
				var v = new Vector2i(i, i);
				result += v.Area();
			}
			Log.Add($"AllocTest#new({ n }): { result } { (Program.Elapsed() - start):0.00}ms");

			result = 0;
			start = Program.Elapsed();
			for (var i = 0; i < n; i++)
			{
				var v = Vector2i.Zero;
				v.X = i;
				v.Y = i;
				result += v.Area();
			}
			Log.Add($"AllocTest#zero+set({ n }): { result } { (Program.Elapsed() - start):0.00}ms");

			result = 0;
			start = Program.Elapsed();
			for (var i = 0; i < n; i++)
			{
				var v = default(Vector2i);
				v.X = i;
				v.Y = i;
				result += v.Area();
			}
			Log.Add($"AllocTest#default({ n }): { result } { (Program.Elapsed() - start):0.00}ms");

			result = 0;
			start = Program.Elapsed();
			for (var i = 0; i < n; i++)
			{
				Vector2i v;
				v.X = i;
				v.Y = i;
				result += v.Area();
			}
			Log.Add($"AllocTest#classic({ n }): { result } { (Program.Elapsed() - start):0.00}ms");

			result = 0;
			start = Program.Elapsed();
			for (var i = 0; i < n; i++)
			{
				var v = default(Vector2i);
				v.Set(i, i);
				result += v.Area();
			}
			Log.Add($"AllocTest#default+Set()({ n }): { result } { (Program.Elapsed() - start):0.00}ms");

			result = 0;
			var v2 = Vector2i.Zero;
			start = Program.Elapsed();
			for (var i = 0; i < n; i++)
			{
				v2.X = i;
				v2.Y = i;
				result += v2.Area();
			}
			Log.Add($"AllocTest#reuse({ n }): { result } { (Program.Elapsed() - start):0.00}ms");

			result = 0;
			start = Program.Elapsed();
			Parallel.For(0, n, (i) =>
			{
				var v = new Vector2i(i, i);
				result += v.Area();
			});
			Log.Add($"AllocTest#new+parallel({ n }): { result } { (Program.Elapsed() - start):0.00}ms");
		}

		protected override void Update()
		{

		}

		protected override void Render()
		{

		}
	}
}
