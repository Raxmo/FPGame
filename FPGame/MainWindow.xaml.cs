using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Threading;

namespace FPGame
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		[DllImport("Kernel32")]
		public static extern void AllocConsole();
		[DllImport("Kernel32")]
		public static extern void FreeConsole();

		WriteableBitmap wbp;
		public MainWindow()
		{
			InitializeComponent();
			AllocConsole();

			CurState = new StateDelegate(AnimateOpening);

			sw.Start();
			DT.Tick += new EventHandler(Update);
			DT.Interval = new TimeSpan(0, 0, 0, 0, 10);
		}

		public byte[,] buffer;
		byte[] dbuffer;

		Random rand = new Random();

		public void INITSCREEN()
		{
			wbp = new WriteableBitmap(
				(int)MWin.ActualWidth,
				(int)MWin.ActualHeight,
				96,
				96,
				PixelFormats.Gray8,
				null);

			MAINIMAGE.Source = wbp;

			buffer = new byte[wbp.PixelWidth, wbp.PixelHeight];
			for(int y = 0; y < buffer.GetLength(1); y++)
			{
				for (int x = 0; x < buffer.GetLength(0); x++)
				{
					buffer[x, y] = 127;
				}
			}
			dbuffer = new byte[buffer.GetLength(0) * buffer.GetLength(1)];
		}


		double ScreenAlpha = 0.0;
		public void DrawScreen()
		{
			rand.NextBytes(dbuffer);

			int i = 0;
			for(int y = 0; y < buffer.GetLength(1); y++)
			{
				for(int x = 0; x < buffer.GetLength(0); x++)
				{
					dbuffer[i] = (byte)(dbuffer[i] < Math.Clamp(buffer[x, y], (byte)1, (byte)254) ? 255.0 * ScreenAlpha : 0);

					i++;
				}
			}

			Int32Rect rect = new Int32Rect(
				0,
				0,
				buffer.GetLength(0),
				buffer.GetLength(1));

			wbp.WritePixels(rect, dbuffer, buffer.GetLength(0), 0);
		}

		DispatcherTimer DT = new DispatcherTimer();

		private void MAINIMAGE_Loaded(object sender, RoutedEventArgs e)
		{
			INITSCREEN();
			DT.Start();
		}

		double deltat = 1.0;
		long totalT = 0;
		Stopwatch sw = new Stopwatch();
		delegate void StateDelegate();
		StateDelegate CurState;
		public void Update(object sender, EventArgs e)
		{
			long NT = sw.ElapsedMilliseconds;
			deltat = (NT - totalT) / 1000.0;
			totalT = NT;

			Console.CursorLeft = 0;
			Console.Write($"FPS: {(int)(1.0 / deltat)}   ");

			CurState();

			DrawScreen();
		}

		void NULLSTATE() { return; }

		double animpoint = -2;
		void AnimateOpening()
		{
			ScreenAlpha = Math.Clamp(animpoint / 10.0, 0, 1);
			animpoint += deltat;

			if (ScreenAlpha == 1.0)
			{
				animpoint = 0.0;
				CurState = FadeInScene;
			}
		}

		double Clarity = 0.0;
		void AnimOpening2()
		{
			for(int y = 0; y < buffer.GetLength(1); y++)
			{
				for(int x = 0; x < buffer.GetLength(0); x++)
				{
					double a = (double)buffer.GetLength(1) / buffer.GetLength(0);
					double u = (x / (double)buffer.GetLength(0) - 0.5) * 2.0;
					double v = (y / (double)buffer.GetLength(1) - 0.5) * 2.0 * a;

					double d = Math.Clamp(2.0 - (Math.Sqrt(u * u + v * v) + animpoint), 0.0, 0.5);

					byte val = (byte)(255 * d);

					buffer[x, y] = val;
				}
			}

			if(animpoint > 2.5)
			{
				CurState = FadeInScene;
			}

			animpoint += deltat / 10.0;
		}

		byte[,] Map =
		{
			{1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
			{1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1 },
			{1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1 },
			{1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
		};

		double playerx = 4;
		double playery = 1.5;
		double playera = -0.5;

		double fov = 1;

		double vdepth = 15;

		void HandleInput()
		{
			double radius = 0.2;

			#region Controlls
			if (Keyboard.IsKeyDown(Key.A))
			{
				playera -= 1 * deltat;
			}
			if (Keyboard.IsKeyDown(Key.D))
			{
				playera += 1 * deltat;
			}

			double DPX = 0;
			double DPY = 0;
			if (Keyboard.IsKeyDown(Key.W))
			{
				DPX += Math.Sin(playera) * deltat;
				DPY += Math.Cos(playera) * deltat;
			}
			if (Keyboard.IsKeyDown(Key.S))
			{
				DPX -= Math.Sin(playera) * deltat;
				DPY -= Math.Cos(playera) * deltat;
			}

			double potpx = playerx + DPX;
			double potpy = playery + DPY;

			int scx = (int)(potpx - 1.0);
			int scy = (int)(potpy - 1.0);
			int ecx = (int)(potpx + 1.0);
			int ecy = (int)(potpy + 1.0);

			for(int cy = scy; cy <= ecy; cy++)
			{
				for(int cx = scx; cx <= ecx; cx++)
				{
					if(Map[cx, cy] == 1)
					{
						double nx = Math.Clamp(potpx, cx, cx + 1);
						double ny = Math.Clamp(potpy, cy, cy + 1);

						double rx = nx - potpx;
						double ry = ny - potpy;
						double rm = Math.Sqrt(rx * rx + ry * ry);
						double nrx = rx / rm;
						double nry = ry / rm;

						double ovlp = radius - rm;
						if (double.IsNaN(ovlp)) ovlp = 0;

						if(ovlp > 0)
						{
							potpx = potpx - nrx * ovlp;
							potpy = potpy - nry * ovlp;
						}
					}
				}
			}

			playerx = potpx;
			playery = potpy;
			#endregion
		}

		void RenderScene(double ALPHA, double CLARITY)
		{
			double astep = fov / buffer.GetLength(0);

			for(int y = buffer.GetLength(1) / 2; y < buffer.GetLength(1); y++)
			{
				int p = (y - (buffer.GetLength(1) / 2));

				double posZ = buffer.GetLength(1);

				double rowa = playera - fov / 2.0;

				double rowdist = posZ / p;
				for(int x = 0; x < buffer.GetLength(0); x++)
				{
					double floorx = playerx + Math.Sin(rowa) * rowdist;
					double floory = playery + Math.Cos(rowa) * rowdist;

					double u = floorx % 1.0;
					double v = floory % 1.0;

					rowa += astep;

					if(Math.Abs(u - 0.5) >= 0.48 || Math.Abs(v - 0.5) >= 0.48 || (Math.Abs(u - 0.5) <= 0.05 && Math.Abs(v - 0.5) <= 0.05))
					{
						buffer[x, y] = 0;
						buffer[x, buffer.GetLength(1) - y - 1] = 0;
					}
					else
					{
						//double alpha = Math.Clamp(1.0 - (rowdist / vdepth), 0.0, 1.0);
						double alpha = Math.Pow(1.0 / 255.0, rowdist / vdepth);
						buffer[x, y] = (byte)(alpha * 255);
						buffer[x, buffer.GetLength(1) - y - 1] = (byte)(alpha * 255);
					}

					
				}
			}

			for (int x = 0; x < buffer.GetLength(0); x++)
			{
				double rayangle = playera - fov / 2.0 + (double)x / buffer.GetLength(0) * fov;
				double dist = 0;

				//REFACTOR
				// Calculate distence to wall
				bool hit = false;
				double ax = Math.Sin(rayangle);
				double ay = Math.Cos(rayangle);
				double sx = Math.Sqrt(1 + (ay * ay) / (ax * ax));
				double sy = Math.Sqrt(1 + (ax * ax) / (ay * ay));
				double dx = 0;
				double dy = 0;
				int checkx = (int)playerx;
				int checky = (int)playery;
				int stepx;
				int stepy;

				if (ax < 0)
				{
					stepx = -1;
					dx = (playerx - (double)checkx) * sx;
				}
				else
				{
					stepx = 1;
					dx = ((double)checkx + 1.0 - playerx) * sx;
				}
				if (ay < 0)
				{
					stepy = -1;
					dy = (playery - (double)checky) * sy;
				}
				else
				{
					stepy = 1;
					dy = ((double)checky - playery + 1) * sy;
				}

				while (!hit && dist < vdepth)
				{
					if (dx < dy)
					{
						checkx += stepx;
						dist = dx;
						dx += sx;
					}
					else
					{
						checky += stepy;
						dist = dy;
						dy += sy;
					}

					if (checkx >= 0 && checkx < Map.GetLength(0) && checky >= 0 && checky < Map.GetLength(1))
					{
						if (Map[checkx, checky] == 1)
						{
							hit = true;
						}
					}
				}



				// Draw columns
				dist = Math.Min(vdepth, dist);
				int ceiling = (int)((double)(buffer.GetLength(1) / 2.0) - buffer.GetLength(1) / (double)(dist));
				int floor = buffer.GetLength(1) - ceiling;

				double u = (playerx + ax * dist) % 1.0;
				double v = (playery + ay * dist) % 1.0;

				double tu = Math.Abs(u - v);
				for (int y = 0; y < buffer.GetLength(1); y++)
				{
					double tv = (double)(y - ceiling) / (floor - ceiling);

					if (y > ceiling && y < floor)
					{
						if (hit && Math.Abs(tu - 0.5) >= 0.48 || Math.Abs(tv - 0.5) >= 0.48 || (Math.Abs(tu - 0.5) <= 0.05 && Math.Abs(tv - 0.5) <= 0.05))
						{
							buffer[x, y] = 0;
						}
						else
						{
							//double alpha = (vdepth - dist) / vdepth;
							double alpha = Math.Pow(1.0 / 255.0, dist / vdepth);
							buffer[x, y] = (byte)(255 * alpha);
						}
					}

					double VAL = buffer[x, y] * ALPHA;
					double c = (1.0 - CLARITY) * 127 + CLARITY * VAL;
					buffer[x, y] = (byte)c;
				}
			}
		}

		double SceneAlpha = 0;
		void FadeInScene()
		{
			RenderScene(1.0, Clarity);
			Clarity += deltat / 7;

			if (Clarity >= 1.0)
			{
				CurState = RunGame;
			}
		}

		void RunGame()
		{

			HandleInput();

			//rendering
			RenderScene(1.0, 1.0);
		}
	}
}
