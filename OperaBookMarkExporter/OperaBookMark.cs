using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Text.RegularExpressions;

namespace OperaBookMarkExporter {
	class OperaBookMark
	{

		enum MODE{
			eNone,
			eCreateFolder,
			eCreateAddress,
		}

		delegate void OneArgDelegate<T>(T arg);

		// アドレス情報
		public class Address
		{
			public string Name = null;
			public string URL = null;

			public bool IsComplete()
			{
				if(Name != null){
					if(URL != null){
						return true;
					}
				}
				return false;
			}
		}

		// フォルダ情報
		public class Folder
		{
			public string			Name = null;	// フォルダ名
			public Folder			Parent = null;					// 親フォルダ
			public List<Folder>		Folders = new List<Folder>();	// フォルダが抱えるフォルダ
			public List<Address>	Addresses = new List<Address>();	// フォルダが抱えるアドレス

			public Folder(Folder parent, string name)
			{
				Parent = parent;
				Name = name;
			}

			public Folder(){}
		}

		// アドレスデータのルート
		public Folder		Root;

		MainWindow Parent = null;

		MODE mMode = MODE.eNone;

		// 出力ルートパス
//		string OutputRootPath = "D:\\data\\program\\OperaBookMarkExporter\\data\\";
		string OutputRootPath = "D:\\program\\OperaBookMarkExporter\\data\\";

		public OperaBookMark(MainWindow parent)
		{
			Parent = parent;
		}

		/// <summary>
		/// IEブックマークにコンバート
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public async Task<bool> ConvertAsync(string path)
		{

			bool result = false;

			Func<bool> ConvertAsyncJob = () => {

				System.Windows.Application.Current.Dispatcher.BeginInvoke(new OneArgDelegate<bool>(Parent.ButtonEnable), false);

				StreamReader reader = new StreamReader(path, Encoding.GetEncoding("utf-8"));

				string text = reader.ReadToEnd();

				reader.Close();

				StringReader StrReader = new StringReader(text);

				Root = new Folder(null, "Root");

				// パース
				ParseOperaBookMark(ref Root, StrReader);


				// エクスポート
				ExportIEBookMark(Root);

				System.Windows.Application.Current.Dispatcher.BeginInvoke(new OneArgDelegate<bool>(Parent.ButtonEnable), true);
				System.Windows.Application.Current.Dispatcher.BeginInvoke(new OneArgDelegate<string>(Parent.WriteText), "完了");

				return result;
			};

		//	try{
				await Task.Run(ConvertAsyncJob);
		//	}
		//	catch(FormatException){
		//		result = false;
		//	}

			return result;

		}


		/// <summary>
		/// オペラブックマークのパース処理
		/// </summary>
		/// <param name="root"></param>
		/// <param name="reader"></param>
		void ParseOperaBookMark(ref Folder root, StringReader reader)
		{
			// 操作用のカレントフォルダ
			Folder CurrentFolder = Root;
			Address WorkAddress = null;

			string work = reader.ReadLine();
			while(work != null){

				switch(mMode){
				case MODE.eNone:
					if(work.Contains("#FOLDER")){
						Folder folder = new Folder();
						folder.Parent = CurrentFolder;
						CurrentFolder.Folders.Add(folder);
						CurrentFolder = folder;

						mMode = MODE.eCreateFolder;
					}
					else if(work.Contains("#URL")){
						Address address = new Address();
						CurrentFolder.Addresses.Add(address);
						WorkAddress = address;

						mMode = MODE.eCreateAddress;
					}
					else if(work.Equals("-")){
						if(CurrentFolder.Parent != null){
							CurrentFolder = CurrentFolder.Parent;
						}
						else{
							if(!CurrentFolder.Name.Equals("Root")){
								// フォーマットが想定外。例外出力してコード組み直し
								throw new FormatException();
							}
						}
					}
					break;
				case MODE.eCreateFolder:
					if(work.Contains("NAME=")){
						string name = Regex.Replace(work, "\tNAME=", "");
						CurrentFolder.Name = name;
						mMode = MODE.eNone;
					}
					break;
				case MODE.eCreateAddress:
					if(work.Contains("NAME=")){
						string name = Regex.Replace(work, "\tNAME=", "");
						WorkAddress.Name = name;
					}
					if(work.Contains("URL=")){
						string url = Regex.Replace(work, "\tURL=", "");
						WorkAddress.URL = url;
					}
					if(WorkAddress.IsComplete()){
						WorkAddress = null;
						mMode = MODE.eNone;
					}
					break;
				}

				work = reader.ReadLine();
			}
		}


		void ExportIEBookMark(Folder root)
		{
			Folder CurrentFolder = root;

			string folderPath = OutputRootPath + CurrentFolder.Name;
			if(!Directory.Exists(folderPath)){
				Directory.CreateDirectory(folderPath);
			}
			OutPutIEBookMark(folderPath, CurrentFolder);

		}

// 参考URL
// ファイル名に使用できない文字
// http://dobon.net/vb/dotnet/file/invalidpathchars.html
// ショートカットファイルの作成
// http://dobon.net/vb/dotnet/file/createshortcut.html


		void OutPutIEBookMark(string outputpath, Folder current)
		{
			// まず子フォルダがあるなら作ってどんどん潜っていく
			foreach(var folder in current.Folders){
				string outputdir = outputpath + "\\" + folder.Name;
				if(!Directory.Exists(outputdir)){
					Directory.CreateDirectory(outputdir);
				}
				OutPutIEBookMark(outputdir, folder);
			}

			foreach(var address in current.Addresses){
				// ファイル名に使えない文字を削除する
				string filename = RemoveChars(address.Name, Path.GetInvalidFileNameChars());
				string filepath = outputpath + "\\" + filename + ".url";

				StreamWriter sw = new StreamWriter(
											filepath,
											false,
											new UTF8Encoding(true));
				string text = "[DEFAULT]\r\nBASEURL="+address.URL;
				sw.WriteLine("[InternetShortcut]");
				sw.WriteLine("URL=" + address.URL);
				sw.Close();
			}

			
		}

		/// <summary>
		/// 指定の文字を文字列から削除する
		/// </summary>
		/// <param name="str"></param>
		/// <param name="chars"></param>
		/// <returns></returns>
		string RemoveChars(string str, char[] chars)
		{
			System.Text.StringBuilder buf = new System.Text.StringBuilder(str);
			foreach(char c in chars){
				buf.Replace(c.ToString(), "");
			}
			return buf.ToString();
		}

	}
}
