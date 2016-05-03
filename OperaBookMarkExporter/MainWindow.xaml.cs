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
using System.IO;

namespace OperaBookMarkExporter {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window {

		
		OperaBookMark	mOperaBookMark;

		public void WriteText(string text)
		{
			Status.Text = text;
		}

		public void ButtonEnable(bool isEnable)
		{
			ExportButton.IsEnabled = isEnable;
		}

		public MainWindow() {
			InitializeComponent();
			mOperaBookMark = new OperaBookMark(this);

		}

		/// <summary>
		/// 変換開始
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void Button_Click(object sender, RoutedEventArgs e) {
			await mOperaBookMark.ConvertAsync(SourcePath.Text, DestPath.Text);
		}

		private void SelectInputPath_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
//			ofd.FileName = "bookmark.adr";
//			ofd.InitialDirectory = 
			ofd.Filter = "OperaBookmark(*.adr)|";
			ofd.Title = "ファイルを選択してください";
			ofd.RestoreDirectory = true;
			if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK){
				SourcePath.Text = ofd.FileName;
			}
		}

		private void SelectOutputPath_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
			fbd.Description = "出力先を選択してください";
			if(fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK){
				DestPath.Text = fbd.SelectedPath;
			}
		}
	}
}
