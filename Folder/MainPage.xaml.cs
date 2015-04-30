﻿using System;
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
			try
			{
				string pathStr = txt_Path.Text;
				pathStr = pathStr.Replace('\\', '/');

				DirectoryInfo dir = new DirectoryInfo(pathStr);

				//分4步保存到数据库
				dbOp.ClearDBList();					//先清空列表
				dbOp.AddFileToList(dir, null);		//本目录下的文件
				dbOp.AddDirToList(dir, null);		//本目录下的子目录(包括文件)
				dbOp.AddListToDB();					//保存到数据库

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
				DirInfoWithID dir = dbOp.DirList[index];

				dbOp.GetFileListFromDB(dir.Info.FullName,dir.Id);
				lst_File.ItemsSource = dbOp.FileList;

				extensions.Clear();
				extensions.Add("ALL");

				foreach (FileInfo file in dbOp.FileList)
				{
					string fileExt = file.Extension;		//后缀
					if (!extensions.Contains(fileExt))
					{
						extensions.Add(fileExt);
					}
				}
			}
		}

 		private void ddlst_Extension_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ObservableCollection<FileInfo> files = dbOp.FileList;

			if (ddlst_Extension.SelectedIndex > 0)
			{
				files.Clear();

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

		private void txt_Path_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				btn_Save_Click(null, null);
			}
		}

		private void btn_Load_Click(object sender, RoutedEventArgs e)
		{
			if (dbOp.DirList.Count != 0)
			{
				lst_Folder.ItemsSource = dbOp.DirList;
				SetEnabled(true);
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
					path = path.Replace('\\', '/');
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
