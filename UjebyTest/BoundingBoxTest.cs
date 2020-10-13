using SDL2;
using Ujeby.Plosinofka.Common;

namespace UjebyTest
{
	class BoundingBoxTest : TestBase
	{
		private Vector2i Mouse;
		private BoundingBox Box;
		private bool DrawLine = false;
		private Vector2f Point1;
		private Vector2f Point2;

		public BoundingBoxTest() : base()
		{
		}

		protected override void Init()
		{
			Box = new BoundingBox()
			{
				Size = (Vector2f)Program.WindowSize / 8,
			};
			Box.Position = (Vector2f)Program.WindowSize / 2 - Box.Size / 2;
		}

		protected override void Update()
		{
			var mouseState = SDL.SDL_GetMouseState(out Mouse.X, out Mouse.Y);
			Mouse.Y = Program.WindowSize.Y - Mouse.Y;

			if (!DrawLine && (mouseState & 1) == 1)
			{
				Point1 = (Vector2f)Mouse;
				Point2 = (Vector2f)Mouse;

				DrawLine = true;
			}

			if (DrawLine)
				Point2 = (Vector2f)Mouse;

			if ((mouseState & 1) != 1)
				DrawLine = false;

			SDL.SDL_SetWindowTitle(Program.WindowPtr, $"Mouse={ Mouse }, state={ mouseState:X}");
		}

		protected override void Render()
		{
			// clear backbuffer
			SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0, 0, 0, 255);
			SDL.SDL_RenderClear(Program.RendererPtr);

			var rect = new SDL.SDL_Rect
			{
				x = (int)Box.Position.X,
				y = Program.WindowSize.Y - (int)Box.Position.Y,
				w = (int)Box.Size.X,
				h = -(int)Box.Size.Y,
			};
			SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0xff, 0xff, 0xff, 0xff);
			SDL.SDL_RenderDrawRect(Program.RendererPtr, ref rect);

			if (DrawLine)
			{
				var distance = (Point2 - Point1).Length();
				var direction = (Point2 - Point1).Normalize();

				var t = Box.Intersect(Point1, direction, out Vector2f n);
				Log.Add($"Box.Intersect(origin={ Point1 }, dir={ direction }): { t:0.00}, normal={ n }");
				if (t < distance && !double.IsInfinity(t))
				{
					var point2 = Point1 + direction * t;
					var point3 = point2 + direction * (distance - t);

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
				}
				else
				{
					SDL.SDL_SetRenderDrawColor(Program.RendererPtr, 0xff, 0, 0, 0xff);
					SDL.SDL_RenderDrawLine(Program.RendererPtr,
						(int)Point1.X, Program.WindowSize.Y - (int)Point1.Y,
						(int)Point2.X, Program.WindowSize.Y - (int)Point2.Y);
				}
			}

			// display backbuffer
			SDL.SDL_RenderPresent(Program.RendererPtr);
		}

		/// <summary>
		/// 2020-10-13 15:10:07.533: BoundingBoxStressTest(10000000): 15,27K intersections per second (without inside checking)
		/// 2020-10-14 00:32:16.894: BoundingBoxStressTest(10000000): 13,03K intersections per second (full implementation)
		/// </summary>
		/// <param name="n"></param>
		public static void BoundingBoxStressTest(long n = 10000000)
		{
			var space = 1000;

			var origins = new Vector2f[n];
			for (var i = 0; i < n; i++)
				origins[i] = new Vector2f(Program.Rng.NextDouble() * space - space / 2, Program.Rng.NextDouble() * space - space / 2);

			var directions = new Vector2f[n];
			for (var i = 0; i < n; i++)
				directions[i] = new Vector2f(Program.Rng.NextDouble() - 0.5, Program.Rng.NextDouble() - 0.5).Normalize();

			var bb = new BoundingBox { Position = new Vector2f(space / -8), Size = new Vector2f(space / 4) };

			var start = Program.Elapsed();
			for (var i = 0; i < n; i++)
			{
				var t = bb.Intersect(origins[i], directions[i], out Vector2f normal);
			}

			Log.Add($"BoundingBoxStressTest({ n }): { (n / (Program.Elapsed() - start) / 1000.0):0.00}K intersections per second");
		}
	}
}
