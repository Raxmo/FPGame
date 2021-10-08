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
		}

		public void INITSCREEN()
		{
			wbp = new WriteableBitmap(
				(int)MWin.ActualWidth/10,
				(int)MWin.ActualHeight/10,
				96,
				96,
				PixelFormats.BlackWhite,
				null);

			MAINIMAGE.Source = wbp;
		}

		private void MAINIMAGE_Loaded(object sender, RoutedEventArgs e)
		{
			INITSCREEN();

			Test();
		}

		public void Test()
		{
			Int32Rect rc = new Int32Rect(0, 0, 1, 1);
			wbp.WritePixels(rc, new byte[] { 255 }, 1, 10, 10);
		}
	}
}
