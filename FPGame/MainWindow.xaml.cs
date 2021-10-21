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
using System.IO;

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
			//AllocConsole();

			CurState = new StateDelegate(Starter);

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
				(int)800,
				(int)450,
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

			zbuff = new double[buffer.GetLength(0), buffer.GetLength(1)];
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

		byte[,] Map =
		{
			{1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
			{1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
			{1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
			{1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1 },
			{1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1 },
			{1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 },
			{1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1 },
			{1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 },
			{1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 },
			{1, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 0, 1, 0, 0, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 },
			{1, 0, 1, 1, 0, 1, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
			{1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1 },
			{1, 0, 0, 0, 0, 0, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
			{1, 0, 0, 0, 0, 0, 1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1 },
			{1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 },
			{1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 1 },
			{1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1 },
			{1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1 },
			{1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1 },
			{1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1 },
			{1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 },
			{1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
		};

		double playerx = 1.5;
		double playery = 7.5;
		double playera = -1.0;

		double fov = 1;

		double vdepth = 7;

		void HandleInput()
		{
			if (Map[(int)playery, (int)playerx] == 2) return;

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
				DPX += Math.Cos(playera) * deltat;
				DPY += Math.Sin(playera) * deltat;
			}
			if (Keyboard.IsKeyDown(Key.S))
			{
				DPX -= Math.Cos(playera) * deltat;
				DPY -= Math.Sin(playera) * deltat;
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
					if(Map[cy, cx] == 1 || (Map[cy, cx] == 2 && !HasSeen))
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

		double[,] zbuff;

		double Epx = 14.5;
		double Epy = 17.5;
		double Efr = 0.0;
		double Esr = 1;
		bool HasSeen = false;

		void RenderScene(double ALPHA, double CLARITY)
		{
			if (Map[(int)playery, (int)playerx] == 2)
			{
				for(int y = 0; y < buffer.GetLength(1); y++)
				{
					for(int x = 0; x < buffer.GetLength(0); x++)
					{
						buffer[x, y] = 127;
					}
				}
				CurState = WinGame;
				return;
			}
			bool AllBlack = true;

			double EfrS = Math.Sqrt(Efr);

			double astep = fov / buffer.GetLength(0);

			double dpex = Epx - playerx;
			double dpey = Epy - playery;
			double dste = dpex * dpex + dpey * dpey;

			double nearestsqrd = Math.Abs(Math.Sin(playera) * (playerx - Epx) - Math.Cos(playera) * (playery - Epy));

			double Ea = Math.Atan2(dpey, dpex) - ((playera + Math.PI) % (Math.PI * 2) - Math.PI);
			double Eax = (Ea / fov) + 0.5;
			Eax *= buffer.GetLength(0);

			double TEST = Eax >= 0 && Eax < buffer.GetLength(0) ? zbuff[(int)Eax, 244] : 0;

			for(int y = buffer.GetLength(1) / 2; y < buffer.GetLength(1); y++)
			{
				int p = (y - (buffer.GetLength(1) / 2));

				double posZ = buffer.GetLength(1);

				double rowa = playera - fov / 2.0;

				double rowdist = posZ / p;
				for(int x = 0; x < buffer.GetLength(0); x++)
				{
					zbuff[x, y] = rowdist;
					zbuff[x, buffer.GetLength(1) - y - 1] = rowdist;

					double floorx = playerx + Math.Cos(rowa) * rowdist;
					double floory = playery + Math.Sin(rowa) * rowdist;

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
						buffer[x, y] = (byte)(255);
						buffer[x, buffer.GetLength(1) - y - 1] = (byte)(255);
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
				double ax = Math.Cos(rayangle);
				double ay = Math.Sin(rayangle);
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

				double U = 0.0;
				double V = 0.0;

				int tile = 0;

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

					if (checkx >= 0 && checkx < Map.GetLength(1) && checky >= 0 && checky < Map.GetLength(0))
					{
						if (Map[checky, checkx] > 0)
						{
							tile = Map[checky, checkx];

							hit = true;

							dist = Math.Min(dist, vdepth);

							U = Math.Clamp((playerx + ax * dist) - checkx, 0.0, 1.0);
							V = Math.Clamp((playery + ay * dist) - checky, 0.0, 1.0);
						}
					}
				}

				// Draw columns
				int ceiling = (int)((double)(buffer.GetLength(1) / 2.0) - buffer.GetLength(1) / (double)(dist));
				int floor = buffer.GetLength(1) - ceiling;
				dist = Math.Min(vdepth, dist);

				for (int y = 0; y < buffer.GetLength(1); y++)
				{
					zbuff[x, y] = Math.Min(zbuff[x, y], dist);

					double tv = (double)(y - ceiling) / (floor - ceiling);
					double tu = U * V + (1 - U) * (1 - V);

					if (y > ceiling && y < floor)
					{
						double col = 1.0;
						if(tile == 1)
						{
							if((tv * 12.0) % 1.0 <= 0.1)
							{
								col = 0.0;
							}
							if((tu * 6.0 + (int)(tv * 12.0) / 2.0) % 1.0 <= 0.1)
							{
								col = 0.0;
							}
						}
						else if(tile == 2)
						{
							col = 0.5;
						}

						buffer[x, y] = (byte)(255 * col);
					}

					double vdist = Math.Clamp(zbuff[x, y] / vdepth, 0.0, 1.0);
					double fog = 1.0 - (vdist * vdist);

					buffer[x, y] = (byte)(buffer[x, y] * fog);

					if(!HasSeen) HasSeen |= Eax > 0 && Eax < buffer.GetLength(0) && dste * 2.0  < vdepth * vdepth && dste < TEST * TEST && nearestsqrd < Esr * Esr;
					else
					{
						
						double vx = playerx + ax * zbuff[x, y];
						double vy = playery + ay * zbuff[x, y];

						double dvx = Epx - vx;
						double dvy = Epy - vy;

						double rs = dvx * dvx + dvy * dvy;

						double fall = 1.0;

						if (rs < Efr)
						{
							double kr = Math.Clamp((rs - Efr + EfrS) / EfrS, 0.0, 1.0);

							double col = kr * buffer[x, y] + (1 - kr) * 0.0;

							buffer[x, y] = (byte)(col);
						}
					}
					double VAL = buffer[x, y] * ALPHA;
					AllBlack &= buffer[x, y] == 0;
					double c = (1.0 - CLARITY) * 127 + CLARITY * VAL;
					buffer[x, y] = (byte)c;

				}
			}

			if (AllBlack)
			{
				Clarity -= deltat / 10.0;
			}
			else
			{
				
			}
			Clarity = Math.Clamp(Clarity, 0.0, 1.0);

			if(Clarity == 0.0 && HasSeen)
			{
				CurState = LooseGame;
			}
		}

		double SceneAlpha = 0;
		void FadeInScene()
		{
			RenderScene(1.0, Clarity);
			Clarity += deltat / 7;

			if (Clarity >= 1.0)
			{
				Clarity = 1.0;
				CurState = RunGame;
			}
		}

		void RunGame()
		{
			HandleInput();

			if (HasSeen)
			{
				Efr += deltat;
				Map[2, 2] = 0;
				MWin.Title = "GET OUT";
			}

			//rendering
			RenderScene(1.0, Clarity);
		}

		double endtimer = 0.0;

		void WinGame()
		{
			MWin.Title = "You've escaped";
			ScreenAlpha -= deltat / 5.0;

			ScreenAlpha = Math.Clamp(ScreenAlpha, 0.0, 1.0);

			endtimer += deltat;

			if (endtimer >= 10.0) MWin.Close();
		}

		void LooseGame()
		{
			MWin.Title = "You've been lost to the void";
			ScreenAlpha = 0.0;
			endtimer += deltat;

			if (endtimer >= 5.0) MWin.Close();
		}

		void Starter()
		{
			MWin.Title = "Hit ENTER to begin...";
			if (Keyboard.IsKeyDown(Key.Enter))
			{
				CurState = AnimateOpening;
				MWin.Title = "Find it...";
			}
		}
	}
}
