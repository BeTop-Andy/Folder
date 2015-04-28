using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using HuaweiSoftware.Folder.FolderWCFReference;
using System.Threading;

namespace HuaweiSoftware.Folder
{
	public class DataBaseOperator
	{
		static private FolderWCFClient webClient;

		public ObservableCollection<string> FileList
		{
			get;
			set;
		}

		public ObservableCollection<string> DirList
		{
			get;
			set;
		}

		static private int id;
		public static int Id
		{
			get
			{
				return DataBaseOperator.id;
			}
			set
			{
				DataBaseOperator.id = value;
			}
		}

		public DataBaseOperator()
		{
			webClient = new FolderWCFClient();

			webClient.GetIdCompleted += ((sender, e) => id = e.Result + 1);
			webClient.GetIdAsync();

			FileList = new ObservableCollection<string>();
			DirList = new ObservableCollection<string>();
		}

		public bool CheckExists(string path, string name)
		{
			webClient.ExistsCompleted += new EventHandler<ExistsCompletedEventArgs>(Exists);
			webClient.ExistsAsync(path, name);
			//webClient.
			return false;
		}

		private void Exists(object sender, ExistsCompletedEventArgs e)
		{
			// e.Result;
		}

		/// <summary>
		/// 获取dir目录下的所有子目录，并加入数据库
		/// </summary>
		/// <param name="dir">“根”目录</param>
		/// <param name="pid">父节点的Id</param>
		public void AddDirToDB(DirectoryInfo dir, int? pid)
		{
			IEnumerable<DirectoryInfo> dirs = dir.EnumerateDirectories();

			foreach (DirectoryInfo di in dirs)
			{
				int tmp_id = id;

				//if (!CheckExists(di.Parent.FullName, di.Name))
				//{
				webClient.AddDirToDBAsync(string.Format("{0}|{1}|{2}", id, pid.HasValue ? pid.Value.ToString() : "NULL", di.FullName));

				id++;
				//}

				AddFileToDB(di, tmp_id);
				AddDirToDB(di, tmp_id);
			}
		}

		/// <summary>
		/// 获取dir目录下的所有文件，并加入数据库
		/// </summary>
		/// <param name="dir">“根”目录</param>
		/// <param name="pid">父节点的Id</param>
		public void AddFileToDB(DirectoryInfo dir, int? pid)
		{
			IEnumerable<FileInfo> files = dir.EnumerateFiles();

			foreach (FileInfo fi in files)
			{
				//if (!CheckExists(fi.DirectoryName, fi.Name))
				//{
				webClient.AddFileToDBAsync(string.Format("{0}|{1}|{2}", id, pid.HasValue ? pid.Value.ToString() : "NULL", fi.FullName));

				id++;
				//}
			}
		}

		public void GetFileFromDB(string path)
		{
			webClient.GetFileFromDBCompleted += ((sender, e) =>
			{
				FileList = e.Result;
				MessageBox.Show("载入完成，请再点击加载按钮");
			});
			webClient.GetFileFromDBAsync(path);
		}

		public void GetDirFromDB(string path)
		{
			webClient.GetDirFromDBCompleted += new EventHandler<GetDirFromDBCompletedEventArgs>(foo1);
			webClient.GetDirFromDBAsync(path);
		}

		private void foo1(object sender,GetDirFromDBCompletedEventArgs e)
		{
			//DirList.Add(e.Result[0]);
			MessageBox.Show(e.Result);
			MessageBox.Show("载入完成，请再点击加载按钮");
		}
	}
}
