using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HuaweiSoftware.Folder
{
	public partial class MainPage : UserControl
	{
		ObservableCollection<string> extensions;		// 后缀名的集合

		DataBaseOperator dbOp;							// 操作数据库的对象

		public MainPage()
		{
			InitializeComponent();

			dbOp = new DataBaseOperator();

			extensions = new ObservableCollection<string>();
			ddlst_Extension.ItemsSource = extensions;
			extensions.Add("ALL");
		}

		private void btn_Save_Click(object sender, RoutedEventArgs e)
		{
			lst_Folder.ItemsSource = null;
			lst_File.ItemsSource = null;

			try
			{
				string pathStr = txt_Path.Text;

				if (!CheckPath(pathStr))
				{
					throw new Exception("路径不合法");
				}

				DirectoryInfo dir = new DirectoryInfo(pathStr);

				// 分4步保存到数据库
				dbOp.ClearDBList();					// 先清空列表
				dbOp.AddFileToList(dir, null);		// 本目录下的文件
				dbOp.AddDirToList(dir, null);		// 本目录下的子目录(包括文件)
				dbOp.AddListToDB();					// 保存到数据库
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);

				txt_Path.Text = "";
				txt_Path.Focus();
				SetEnabled(false);
				return;
			}
		}

		/// <summary>
		/// 检查路径是否以":"或":\"或":/"结尾
		/// </summary>
		/// <param name="path">路径</param>
		/// <returns></returns>
		private bool CheckPath(string path)
		{
			if (path.EndsWith(":") || path.EndsWith(":\\") || path.EndsWith(":/"))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// 设置那3个控件的enabled
		/// </summary>
		/// <param name="b"></param>
		private void SetEnabled(bool b)
		{
			txt_Search.IsEnabled = b;
			btn_Search.IsEnabled = b;
			ddlst_Extension.IsEnabled = b;
		}

		private void lst_Folder_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int index = lst_Folder.SelectedIndex;

			if (index >= 0)
			{
				DirNameWithID dir = dbOp.DirList[index];

				dbOp.GetFileListFromDB(dir.Id);

				dbOp.onLoadFileFinish -= new EventHandler(LoadFileFinish);
				dbOp.onLoadFileFinish += new EventHandler(LoadFileFinish);
			}
		}

 		private void ddlst_Extension_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// 临时集合
			ObservableCollection<FileInfo> files = new ObservableCollection<FileInfo>();

			// 选择后缀
			if (ddlst_Extension.SelectedIndex > 0)
			{
				string extension = ddlst_Extension.SelectedValue.ToString();

				foreach (var fileName in dbOp.FileList)
				{
					if (fileName.Extension == extension)
					{
						files.Add(fileName);
					}
				}
			}

			lst_File.ItemsSource = files;
 		}

		private void btn_Search_Click(object sender, RoutedEventArgs e)
		{
			string keyword = txt_Search.Text.ToLower();		// 忽略大小写

			if (keyword != string.Empty)
			{
				ObservableCollection<FileInfo> files = new ObservableCollection<FileInfo>();

				foreach (var file in dbOp.FileList)
				{
					if (file.Name.ToLower().Contains(keyword))
					{
						files.Add(file);
					}
				}

				lst_File.ItemsSource = files;
			}
			else
			{
				ddlst_Extension.SelectedIndex = 0;
			}
 		}

		private void txt_Search_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				btn_Search_Click(null, null);
			}
		}

		private void txt_Search_TextChanged(object sender, TextChangedEventArgs e)
		{
			btn_Search_Click(null, null);
		}

		private void btn_Load_Click(object sender, RoutedEventArgs e)
		{
			dbOp.GetDirFromDB();		// 从目录中读取

			dbOp.onLoadDirFinish += new EventHandler(LoadDirFinish);	// 订阅事件
		}

		/// <summary>
		/// 读取完成，绑定数据源
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LoadDirFinish(object sender, EventArgs e)
		{
			lst_Folder.ItemsSource = dbOp.DirList;
			SetEnabled(true);
		}

		/// <summary>
		/// 读取完成，绑定数据源，添加后缀到选择后缀下拉框
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LoadFileFinish(object sender, EventArgs e)
		{
			lst_File.ItemsSource = dbOp.FileList;

			extensions.Clear();
			extensions.Add("ALL");

			foreach (FileInfo file in dbOp.FileList)
			{
				string fileExt = file.Extension;		// 后缀
				if (!extensions.Contains(fileExt))
				{
					extensions.Add(fileExt);
				}
			}
		}
	}
}
