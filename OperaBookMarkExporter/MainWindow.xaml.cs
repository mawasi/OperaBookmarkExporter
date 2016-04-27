using System;
using System.Collections.Generic;
using System.Linq;
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

namespace OperaBookMarkExporter {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window {

		
		OperaBookMark	mOperaBookMark;

		public void WriteText(string text)
		{
			log.Text = text;
		}

		public void ButtonEnable(bool isEnable)
		{
			ExportButton.IsEnabled = isEnable;
		}

		public MainWindow() {
			InitializeComponent();
			mOperaBookMark = new OperaBookMark(this);

//			SourcePath.Text = "D:\\data\\program\\OperaBookMarkExporter\\data\\source\\bookmark.adr";
			SourcePath.Text = "D:\\program\\OperaBookMarkExporter\\data\\source\\bookmark.adr";
		}

		private async void Button_Click(object sender, RoutedEventArgs e) {
			await mOperaBookMark.ConvertAsync(SourcePath.Text);
		}
	}
}
