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
			DT.Interval = new TimeSpan(0, 0, 0, 0, 15);
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
		Stopwatch sw = new Stopwatch();
		delegate void StateDelegate();
		StateDelegate CurState;
		public void Update(object sender, EventArgs e)
		{
			deltat = sw.ElapsedMilliseconds / 1000.0;
			sw.Restart();

			CurState();

			DrawScreen();
		}

		void NULLSTATE() { return; }

		double animpoint = -2;
		void AnimateOpening()
		{
			Console.Write($"AnimOpening {animpoint}");
			Console.CursorLeft = 0;

			ScreenAlpha = Math.Clamp(animpoint / 10.0, 0, 1);
			animpoint += deltat;

			if (animpoint > 10.0)
			{
				animpoint = 0.0;
				CurState = AnimOpening2;
			}
		}

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

			if(animpoint > 3.0)
			{
				CurState = NULLSTATE;
			}

			animpoint += deltat / 10.0;
		}
	}
}
