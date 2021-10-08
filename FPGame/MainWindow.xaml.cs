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
			dbuffer = new byte[buffer.GetLength(0) * buffer.GetLength(1)];
		}

		public void DrawScreen()
		{
			rand.NextBytes(dbuffer);

			int i = 0;
			for(int y = 0; y < buffer.GetLength(1); y++)
			{
				for(int x = 0; x < buffer.GetLength(0); x++)
				{
					dbuffer[i] = (byte)(dbuffer[i] < buffer[x, y] ? 255 : 0);

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

			for(int y = 0; y < buffer.GetLength(1); y++)
			{
				for(int x = 0; x < buffer.GetLength(0); x++)
				{
					double u = (double)x / buffer.GetLength(0);
					double v = (double)y / buffer.GetLength(1);
					double a = (double)buffer.GetLength(1) / buffer.GetLength(0);

					double px = u - 0.5;
					double py = (v - 0.5) * a;

					buffer[x, y] = (byte)(255 * Math.Sqrt(px * px + py * py));
				}
			}
		}

		public void Update(object sender, EventArgs e)
		{
			DrawScreen();
		}
	}
}
