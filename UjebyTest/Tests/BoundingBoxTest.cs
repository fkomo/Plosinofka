using SDL2;
using System.Collections.Generic;
using System.Linq;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game;

namespace UjebyTest
{
	class AABBTest : TestBase
	{
		private const long TEST_COUNT = 1000000;

		private Vector2i Mouse;
		private Vector2i TmpMouse;
		private uint MouseState;

		private Triangle TmpTriangle;

		private bool DrawingRay = false;
		private Ray TmpRay;

		private bool DrawingBox = false;
		private AABB TmpBox;

		private readonly List<AABB> Boxes = new List<AABB>();
		private Level TestLevel = null;

		public AABBTest() : base()
		{
		}

		protected override void Init()
		{
			AABBTest.TraceTest();
			AABBTest.IntersectTest();
			//AABBTest.RayMarchingTest();
			AABBTest.OverlapTest();

			//Boxes.Add(new AABB(new Vector2f(0, 0), new Vector2f(1920, 15)));

			TestLevel = new Level("test", Boxes.ToArray());

			TmpTriangle = new Triangle(
				new Vector2f(100, 200),
				new Vector2f(500, 300),
				new Vector2f(300, 500)
			);
			var result = TmpTriangle.Overlap(new AABB(new Vector2f(342, 90), new Vector2f(671, 232)));

			//var t = TestLevel.Trace(
			//	new AABB(new Vector2f(309 + 8, 15), new Vector2f(309 + 8 + 16, 15 + 31)),
			//	new Vector2f(4, -2).Normalize(),
			//	out Vector2f normal);
		}

		protected override void Update()
		{
			MouseState = SDL.SDL_GetMouseState(out Mouse.X, out Mouse.Y);
			Mouse.Y = Program.WindowSize.Y - Mouse.Y;

			var leftMouse = (MouseState & 1) == 1;
			var rightMouse = (MouseState & 4) == 4;

			if (!DrawingRay && rightMouse)
				TmpRay = new Ray(Mouse, Vector2f.Zero, true);

			DrawingRay = rightMouse;
			if (DrawingRay)
				TmpRay = new Ray(TmpRay.Origin, Mouse - TmpRay.Origin);

			if (!DrawingBox && leftMouse)
			{
				TmpMouse = Mouse;
				TmpBox = new AABB(Mouse, Mouse);
			}
			else if (DrawingBox && !leftMouse)
			{
				Log.Add($"box: { TmpBox }");
				Boxes.Add(TmpBox);
				TestLevel = new Level("test", Boxes.ToArray());
			}

			DrawingBox = leftMouse;
			if (DrawingBox)
				TmpBox = new AABB(Vector2i.Min(TmpMouse, Mouse), Vector2i.Max(TmpMouse, Mouse));
		}

		protected override void Render()
		{
			var title = $"mouse_state={ MouseState } |";

			// clear backbuffer
			SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0, 0, 0, 255);
			SDL.SDL_RenderClear(Program.RendererPtr);

			// draw boxes
			foreach (var bb in Boxes)
			{
				var rect = new SDL.SDL_Rect
				{
					x = (int)bb.Min.X,
					y = Program.WindowSize.Y - (int)bb.Min.Y,
					w = (int)bb.Size.X,
					h = -(int)bb.Size.Y,
				};
				SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0xff, 0xff, 0xff, 0xff);
				SDL.SDL_RenderDrawRect(Program.RendererPtr, ref rect);
			}

			// draw triangle
			SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0x0, 0xff, 0xff, 0xff);
			SDL.SDL_RenderDrawLine(Program.RendererPtr,
				(int)TmpTriangle.V1.X, Program.WindowSize.Y - (int)TmpTriangle.V1.Y,
				(int)TmpTriangle.V2.X, Program.WindowSize.Y - (int)TmpTriangle.V2.Y);
			SDL.SDL_RenderDrawLine(Program.RendererPtr,
				(int)TmpTriangle.V1.X, Program.WindowSize.Y - (int)TmpTriangle.V1.Y,
				(int)TmpTriangle.V3.X, Program.WindowSize.Y - (int)TmpTriangle.V3.Y);
			SDL.SDL_RenderDrawLine(Program.RendererPtr,
				(int)TmpTriangle.V3.X, Program.WindowSize.Y - (int)TmpTriangle.V3.Y,
				(int)TmpTriangle.V2.X, Program.WindowSize.Y - (int)TmpTriangle.V2.Y);

			if (DrawingRay)
			{
				var rayLength = (Mouse - TmpRay.Origin).Length();

				var trace = TestLevel.Trace(TmpRay.Origin, TmpRay.Direction, out Vector2f n);
				Log.Add($"Level.Intersect(origin={ TmpRay.Origin }, dir={ TmpRay.Direction }): { trace:0.00}, normal={ n }");

				//var tMarch = TestLevel.RayMarch(Point1, direction, out Vector2f n2);
				//Log.Add($"Level.RayMarch(origin={ Point1 }, dir={ direction }): { tMarch:0.00}, normal={ n }");

				var intersect = TestLevel.Intersect(TmpRay);

				title += $" origin={ TmpRay.Origin } | dir={ TmpRay.Direction } | t={ trace:0.00} | mouse={ Mouse } |";
				if (intersect)
					title += " intersect |";

				if (trace < rayLength && !double.IsInfinity(trace))
				{
					var tracePoint = TmpRay.Origin + TmpRay.Direction * trace;

					// origin - trace
					SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0xff, 0xff, 0, 0xff);
					SDL.SDL_RenderDrawLine(Program.RendererPtr,
						(int)TmpRay.Origin.X, Program.WindowSize.Y - (int)TmpRay.Origin.Y,
						(int)tracePoint.X, Program.WindowSize.Y - (int)tracePoint.Y);

					// trace - mouse
					SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0xff, 0, 0, 0xff);
					SDL.SDL_RenderDrawLine(Program.RendererPtr,
						(int)tracePoint.X, Program.WindowSize.Y - (int)tracePoint.Y,
						(int)Mouse.X, Program.WindowSize.Y - (int)Mouse.Y);

					// hit normal
					SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0, 0, 0xff, 0xff);
					SDL.SDL_RenderDrawLine(Program.RendererPtr,
						(int)tracePoint.X, Program.WindowSize.Y - (int)tracePoint.Y,
						(int)tracePoint.X + (int)n.X * 100, Program.WindowSize.Y - (int)(tracePoint.Y + (int)n.Y * 100));
				}
				else
				{
					SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0xff, 0, 0, 0xff);
					SDL.SDL_RenderDrawLine(Program.RendererPtr,
						(int)TmpRay.Origin.X, Program.WindowSize.Y - (int)TmpRay.Origin.Y,
						(int)Mouse.X, Program.WindowSize.Y - (int)Mouse.Y);
				}
			}

			if (DrawingBox)
			{
				var rect = new SDL.SDL_Rect
				{
					x = (int)TmpBox.Min.X,
					y = Program.WindowSize.Y - (int)TmpBox.Min.Y,
					w = (int)TmpBox.Size.X,
					h = -(int)TmpBox.Size.Y,
				};

				if (TestLevel.Overlap(TmpBox) || TmpTriangle.Overlap(TmpBox))
				{
					title += " overlaps |";
					SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0xff, 0x00, 0x00, 0xff);
				}
				else
					SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0x7f, 0x7f, 0x7f, 0xff);

				SDL.SDL_RenderDrawRect(Program.RendererPtr, ref rect);
			}

			SDL.SDL_SetWindowTitle(Program.WindowPtr, title);

			// display backbuffer
			SDL.SDL_RenderPresent(Program.RendererPtr);
		}

		/// <summary>
		/// debug:2020-10-13 15:10:07.533: AABB.Trace(10000000): 15,27M per second (without inside checking)
		/// debug:2020-10-14 00:32:16.894: AABB.Trace(10000000): 13,03M per second (full implementation)
		/// release:2020-10-15 08:42:44.513: AABB.Trace(10000000): 26,67M per second
		/// release:2020-10-16 17:01:20.992: AABB.Trace(10000000): 26,72M per second
		/// release:2020-10-21 16:02:48.634: AABB.Trace(10000000): 24,21M per second
		/// 
		/// </summary>
		/// <param name="n"></param>
		public static void TraceTest(long n = TEST_COUNT)
		{
			var space = 1000;

			var origins = new Vector2f[n];
			for (var i = 0; i < n; i++)
				origins[i] = new Vector2f(
					Program.Rng.NextDouble() * space - space / 2,
					Program.Rng.NextDouble() * space - space / 2);

			var directions = new Vector2f[n];
			for (var i = 0; i < n; i++)
				directions[i] = new Vector2f(Program.Rng.NextDouble() - 0.5, Program.Rng.NextDouble() - 0.5)
					.Normalize();

			var bb = new AABB(new Vector2f(space / -8), new Vector2f(space / -8) + new Vector2f(space / 4));

			var start = Program.Elapsed();
			for (var i = 0; i < n; i++)
				bb.Trace(origins[i], directions[i], out _);

			Log.Add($"AABB.Trace({ n }): { (n / (Program.Elapsed() - start) / 1000.0):0.00}M per second");
		}

		/// <summary>
		/// release:2020-10-24 23:59:07.636: AABB.Intersects(10000000): 18,97M per second
		/// </summary>
		/// <param name="n"></param>
		public static void IntersectTest(long n = TEST_COUNT)
		{
			var space = 1000;

			var origins = new Vector2f[n];
			for (var i = 0; i < n; i++)
				origins[i] = new Vector2f(
					Program.Rng.NextDouble() * space - space / 2,
					Program.Rng.NextDouble() * space - space / 2);

			var directions = new Vector2f[n];
			for (var i = 0; i < n; i++)
				directions[i] = new Vector2f(Program.Rng.NextDouble() - 0.5, Program.Rng.NextDouble() - 0.5)
					.Normalize();

			var bb = new AABB(new Vector2f(space / -8), new Vector2f(space / -8) + new Vector2f(space / 4));

			var start = Program.Elapsed();
			for (var i = 0; i < n; i++)
				bb.Intersect(new Ray(origins[i], directions[i], true));

			Log.Add($"AABB.Intersects({ n }): { (n / (Program.Elapsed() - start) / 1000.0):0.00}M per second");
		}

		/// <summary>
		/// release:2020-10-16 17:01:32.824: AABB.RayMarch(10000000) : 0,88M per second
		/// release:2020-10-21 16:03:01.240: AABB.RayMarch(10000000) : 0,82M per second
		/// </summary>
		/// <param name="n"></param>
		public static void RayMarchingTest(long n = TEST_COUNT)
		{
			var space = 1000;

			var origins = new Vector2f[n];
			for (var i = 0; i < n; i++)
				origins[i] = new Vector2f(
					Program.Rng.NextDouble() * space - space / 2,
					Program.Rng.NextDouble() * space - space / 2);

			var directions = new Vector2f[n];
			for (var i = 0; i < n; i++)
				directions[i] = new Vector2f(Program.Rng.NextDouble() - 0.5, Program.Rng.NextDouble() - 0.5)
					.Normalize();

			var objects = new AABB[]
			{
				new AABB(new Vector2f(space / -8), new Vector2f(space / -8) + new Vector2f(space / 4))
			}.Select(a => a as ISdf).ToArray();

			var start = Program.Elapsed();
			for (var i = 0; i < n; i++)
				RayMarching.Distance(objects, origins[i], directions[i], out Vector2f normal);

			Log.Add($"AABB.RayMarch({ n }): { (n / (Program.Elapsed() - start) / 1000.0):0.00}M per second");
		}

		/// <summary>
		/// release:2020-10-24 23:59:09.676: AABB.Overlaps(10000000): 63,08M per second
		/// </summary>
		/// <param name="n"></param>
		public static void OverlapTest(long n = TEST_COUNT)
		{
			var space = 10000;

			var bbs1 = new List<AABB>();
			for (var i = 0; i < n; i++)
				bbs1.Add(new AABB(new Vector2f(Program.Rng.NextDouble() * space, Program.Rng.NextDouble() * space),
					new Vector2f(Program.Rng.NextDouble() * space, Program.Rng.NextDouble() * space)));
			var bbs2 = new List<AABB>();
			for (var i = 0; i < n; i++)
				bbs2.Add(new AABB(new Vector2f(Program.Rng.NextDouble() * space, Program.Rng.NextDouble() * space),
					new Vector2f(Program.Rng.NextDouble() * space, Program.Rng.NextDouble() * space)));

			var start = Program.Elapsed();
			for (var i = 0; i < n; i++)
				bbs1[i].Overlap(bbs2[i]);
			Log.Add($"AABB.Overlaps({ n }): { (n / (Program.Elapsed() - start) / 1000.0):0.00}M per second");

			//start = Program.Elapsed();
			//for (var i = 0; i < n; i++)
			//	bbs1[i].Overlaps2(bbs2[i]);
			//Log.Add($"AABB.Overlaps2({ n }): { (n / (Program.Elapsed() - start) / 1000.0):0.00}M per second");
		}
	}
}
