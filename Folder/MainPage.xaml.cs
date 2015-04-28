using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HuaweiSoftware.Folder.FolderWCFReference;


namespace HuaweiSoftware.Folder
{
	public partial class MainPage : UserControl
	{
		ObservableCollection<PrefixFolder> folders;		// 文件夹的相关信息
		ObservableCollection<string> extensions;		// 后缀名的集合

		DataBaseOperator dbOp;							// 操作数据库的对象

		public MainPage()
		{
			InitializeComponent();
			folders = new ObservableCollection<PrefixFolder>();
			lst_Folder.ItemsSource = folders;

			dbOp = new DataBaseOperator();

			extensions = new ObservableCollection<string>();
			ddlst_Extension.ItemsSource = extensions;
			extensions.Add("ALL");
		}

		private void btn_Save_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				string pathStr = txt_Path.Text;

				DirectoryInfo dir = new DirectoryInfo(pathStr);

				folders.Clear();
				folders.Add(new PrefixFolder(".", dir));

				GetAllDir(dir, 0);
				dbOp.AddFileToDB(dir, null);
				dbOp.AddDirToDB(dir, null);

				DataBaseOperator.Id++;		// 为了分隔开

				SetEnabled(true);

				//MessageBox.Show("保存成功");
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

		private void SetEnabled(bool b)
		{
			txt_Search.IsEnabled = b;
			btn_Search.IsEnabled = b;
			ddlst_Extension.IsEnabled = b;
		}

		/// <summary>
		/// 获取dir目录下的所有子目录，并加入folders集合
		/// </summary>
		/// <param name="dir">“根”目录</param>
		/// <param name="level">深度</param>
		private void GetAllDir(DirectoryInfo dir, int level)
		{
			IEnumerable<DirectoryInfo> dirs = dir.EnumerateDirectories();
			string str = "";

			for (int i = 0; i < level; i++)
			{
				str += "/ ";
			}

			foreach (DirectoryInfo di in dirs)
			{
				folders.Add(new PrefixFolder(str, di));
				GetAllDir(di, level + 1);
			}
		}



		private void lst_Folder_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int index = lst_Folder.SelectedIndex;

			if (index >= 0)
			{
				DirectoryInfo di = folders[index].DirInfo;
				IEnumerable<FileInfo> files = di.EnumerateFiles();

				lst_File.Items.Clear();
				extensions.Clear();
				extensions.Add("ALL");

				foreach (FileInfo i in files)
				{
					lst_File.Items.Add(i.Name);
					if (!extensions.Contains(i.Extension))
					{
						extensions.Add(i.Extension);
					}
				}
			}
		}

		private void ddlst_Extension_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			DirectoryInfo dir = folders[lst_Folder.SelectedIndex < 0 ?
				0 : lst_Folder.SelectedIndex].DirInfo;
			IEnumerable<FileInfo> files = dir.EnumerateFiles();
			lst_File.Items.Clear();

			// 选择“ALL”
			if (ddlst_Extension.SelectedIndex == 0)
			{
				foreach (var i in files)
				{
					lst_File.Items.Add(i.Name);
				}
			}

			if (ddlst_Extension.SelectedIndex > 0)
			{
				string extension = ddlst_Extension.SelectedValue.ToString();

				foreach (var i in files)
				{
					if (i.Extension == extension)
					{
						lst_File.Items.Add(i.Name);
					}
				}
			}
		}

		private void btn_Search_Click(object sender, RoutedEventArgs e)
		{
			string keyword = txt_Search.Text.ToLower();		// 忽略大小写
			if (keyword != null)
			{
				DirectoryInfo dir = folders[lst_Folder.SelectedIndex < 0 ?
					0 : lst_Folder.SelectedIndex].DirInfo;
				IEnumerable<FileInfo> files = dir.EnumerateFiles();
				lst_File.Items.Clear();

				foreach (var i in files)
				{
					if (i.Name.ToLower().Contains(keyword))
					{
						lst_File.Items.Add(i.Name);
					}
				}
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

		private void txt_Path_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				string pathStr = txt_Path.Text;

				lst_File.Items.Clear();
				folders.Clear();

				try
				{
					DirectoryInfo dir = new DirectoryInfo(pathStr);

					txt_Path.Text = dir.FullName;

					folders.Clear();
					folders.Add(new PrefixFolder(".", dir));

					GetAllDir(dir, 0);

					IEnumerable<FileInfo> files = dir.EnumerateFiles();

					lst_File.Items.Clear();

					foreach (FileInfo i in files)
					{
						lst_File.Items.Add(i.Name);
						if (!extensions.Contains(i.Extension))
						{
							extensions.Add(i.Extension);
						}
					}

					SetEnabled(true);
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
		}

		private void btn_Load_Click(object sender, RoutedEventArgs e)
		{
			if (dbOp.DirList.Count != 0)
			{
				lst_Folder.ItemsSource = dbOp.DirList;
			}
			else
			{
				try
				{
					string path = txt_Path.Text;
					if (!Directory.Exists(path))
					{
						throw new Exception("目录不存在");
					}

					dbOp.GetDirFromDB(path);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
		}
	}
}
