using SDL2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using Ujeby.Plosinofka;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Entities;

namespace UjebyTest
{
	class BoundingBoxTest : TestBase
	{
		private Vector2i Mouse;

		private bool DrawingRay = false;
		private Ray TmpRay;

		private bool DrawingBox = false;
		private BoundingBox TmpBox;

		private List<BoundingBox> Boxes = new List<BoundingBox>();

		public BoundingBoxTest() : base()
		{
		}

		protected override void Init()
		{
			BoundingBoxTest.BoundingBoxTraceTest();
			BoundingBoxTest.BoundingBoxIntersectTest();
			BoundingBoxTest.BoundingBoxRayMarchingTest();
			BoundingBoxTest.BoundingBoxOverlapTest();
		}

		protected override void Update()
		{
			var mouseState = SDL.SDL_GetMouseState(out Mouse.X, out Mouse.Y);
			Mouse.Y = Program.WindowSize.Y - Mouse.Y;

			var leftMouse = (mouseState & 1) == 1;
			var rightMouse = (mouseState & 2) == 2;

			if (!DrawingRay && rightMouse)
				TmpRay = new Ray((Vector2f)Mouse, Vector2f.Zero, true);

			DrawingRay = rightMouse;
			if (DrawingRay)
				TmpRay = new Ray(TmpRay.Origin, (Vector2f)Mouse - TmpRay.Origin);

			if (!DrawingBox && leftMouse)
				TmpBox = new BoundingBox((Vector2f)Mouse, (Vector2f)Mouse);

			if ()

			if (DrawingBox)
				TmpBox = new BoundingBox();


			Boxes.Add(TmpBox);

			if (!leftMouse)
				DrawingBox = false;

			if (DrawingRay)
				TmpRay = new Ray(TmpRay.Origin, (Vector2f)Mouse - TmpRay.Origin);

		}

		protected override void Render()
		{
			// clear backbuffer
			SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0, 0, 0, 255);
			SDL.SDL_RenderClear(Program.RendererPtr);

			foreach (var bb in TestLevel.Colliders)
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

			if (DrawLine)
			{
				var distance = (Point2 - Point1).Length();
				var direction = (Point2 - Point1).Normalize();

				var ray = new Ray(Point1, direction, true);

				var tTrace = TestLevel.Trace(Point1, direction, out Vector2f n);
				Log.Add($"Level.Intersect(origin={ Point1 }, dir={ direction }): { tTrace:0.00}, normal={ n }");

				//var tMarch = TestLevel.RayMarch(Point1, direction, out Vector2f n2);
				//Log.Add($"Level.RayMarch(origin={ Point1 }, dir={ direction }): { tMarch:0.00}, normal={ n }");

				var intersection = TestLevel.Intersect(ray);

				var title = $"p1={ Point1 } | t={ tTrace:0.00} | p2={ Point2 } |";
				if (intersection)
					title += " intersection |";

				if (tTrace < distance && !double.IsInfinity(tTrace))
				{
					var point2 = Point1 + direction * tTrace;
					var point3 = point2 + direction * (distance - tTrace);

					SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0xff, 0xff, 0, 0xff);
					SDL.SDL_RenderDrawLine(Program.RendererPtr,
						(int)Point1.X, Program.WindowSize.Y - (int)Point1.Y,
						(int)point2.X, Program.WindowSize.Y - (int)point2.Y);

					SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0xff, 0, 0, 0xff);
					SDL.SDL_RenderDrawLine(Program.RendererPtr,
						(int)point2.X, Program.WindowSize.Y - (int)point2.Y,
						(int)point3.X, Program.WindowSize.Y - (int)point3.Y);

					// hit normal
					SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0, 0, 0xff, 0xff);
					SDL.SDL_RenderDrawLine(Program.RendererPtr,
						(int)point2.X, Program.WindowSize.Y - (int)point2.Y,
						(int)point2.X + (int)n.X * 100, Program.WindowSize.Y - (int)(point2.Y + (int)n.Y * 100));

					title += $" p3={ point3 } |";
				}
				else
				{
					SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0xff, 0, 0, 0xff);
					SDL.SDL_RenderDrawLine(Program.RendererPtr,
						(int)Point1.X, Program.WindowSize.Y - (int)Point1.Y,
						(int)Point2.X, Program.WindowSize.Y - (int)Point2.Y);
				}

				SDL.SDL_SetWindowTitle(Program.WindowPtr, title);
			}
			else
				SDL.SDL_SetWindowTitle(Program.WindowPtr, $"mouse={ Mouse }");

			// display backbuffer
			SDL.SDL_RenderPresent(Program.RendererPtr);
		}

		/// <summary>
		/// debug:2020-10-13 15:10:07.533: BoundingBox.Trace(10000000): 15,27M per second (without inside checking)
		/// debug:2020-10-14 00:32:16.894: BoundingBox.Trace(10000000): 13,03M per second (full implementation)
		/// release:2020-10-15 08:42:44.513: BoundingBox.Trace(10000000): 26,67M per second
		/// release:2020-10-16 17:01:20.992: BoundingBox.Trace(10000000): 26,72M per second
		/// release:2020-10-21 16:02:48.634: BoundingBox.Trace(10000000): 24,21M per second
		/// 
		/// </summary>
		/// <param name="n"></param>
		public static void BoundingBoxTraceTest(long n = 10000000)
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

			var bb = new BoundingBox(new Vector2f(space / -8), new Vector2f(space / -8) + new Vector2f(space / 4));

			var start = Program.Elapsed();
			for (var i = 0; i < n; i++)
				bb.Trace(origins[i], directions[i], out Vector2f normal);

			Log.Add($"BoundingBox.Trace({ n }): { (n / (Program.Elapsed() - start) / 1000.0):0.00}M per second");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="n"></param>
		public static void BoundingBoxIntersectTest(long n = 10000000)
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

			var bb = new BoundingBox(new Vector2f(space / -8), new Vector2f(space / -8) + new Vector2f(space / 4));

			var start = Program.Elapsed();
			for (var i = 0; i < n; i++)
				bb.Intersects(new Ray(origins[i], directions[i], true));

			Log.Add($"BoundingBox.Intersects({ n }): { (n / (Program.Elapsed() - start) / 1000.0):0.00}M per second");
		}

		/// <summary>
		/// release:2020-10-16 17:01:32.824: BoundingBox.RayMarch(10000000) : 0,88M per second
		/// release:2020-10-21 16:03:01.240: BoundingBox.RayMarch(10000000) : 0,82M per second
		/// </summary>
		/// <param name="n"></param>
		public static void BoundingBoxRayMarchingTest(long n = 1000000)
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

			var bb = new BoundingBox(new Vector2f(space / -8), new Vector2f(space / -8) + new Vector2f(space / 4));
			var level = new Ujeby.Plosinofka.Level("test-room", new BoundingBox[]
				{
					bb,
				});

			var start = Program.Elapsed();
			for (var i = 0; i < n; i++)
				level.RayMarch(origins[i], directions[i], out Vector2f normal);

			Log.Add($"BoundingBox.RayMarch({ n }): { (n / (Program.Elapsed() - start) / 1000.0):0.00}M per second");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="n"></param>
		public static void BoundingBoxOverlapTest(long n = 1000000)
		{
			var space = 10000;

			var bbs1 = new List<BoundingBox>();
			for (var i = 0; i < n; i++)
				bbs1.Add(new BoundingBox(new Vector2f(Program.Rng.NextDouble() * space, Program.Rng.NextDouble() * space),
					new Vector2f(Program.Rng.NextDouble() * space, Program.Rng.NextDouble() * space)));
			var bbs2 = new List<BoundingBox>();
			for (var i = 0; i < n; i++)
				bbs2.Add(new BoundingBox(new Vector2f(Program.Rng.NextDouble() * space, Program.Rng.NextDouble() * space),
					new Vector2f(Program.Rng.NextDouble() * space, Program.Rng.NextDouble() * space)));

			var start = Program.Elapsed();
			for (var i = 0; i < n; i++)
				bbs1[i].Overlaps(bbs2[i]);
			Log.Add($"BoundingBox.Overlaps({ n }): { (n / (Program.Elapsed() - start) / 1000.0):0.00}M per second");

			start = Program.Elapsed();
			for (var i = 0; i < n; i++)
				bbs1[i].Overlaps2(bbs2[i]);
			Log.Add($"BoundingBox.Overlaps2({ n }): { (n / (Program.Elapsed() - start) / 1000.0):0.00}M per second");
		}
	}
}
