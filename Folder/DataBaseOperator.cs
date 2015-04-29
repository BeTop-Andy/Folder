using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

using HuaweiSoftware.Folder.FolderWCFReference;

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

		public ObservableCollection<DirInfoWithID> DirList
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
			DirList = new ObservableCollection<DirInfoWithID>();
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


		/// <summary>
		/// 根据目录ID从数据库中读取该目录下的文件
		/// </summary>
		/// <param name="id">目录ID</param>
		public void GetFileFromDB(int id)
		{
			webClient.GetFileFromDBCompleted -= new EventHandler<GetFileFromDBCompletedEventArgs>(GetFileFromDBCompleted);
			webClient.GetFileFromDBCompleted += new EventHandler<GetFileFromDBCompletedEventArgs>(GetFileFromDBCompleted);

			int? pid;

			// 把ID转换成PID
			if (id == 0)
			{
				pid = null;
			}
			else
			{
				pid = id;
			}

			webClient.GetFileFromDBAsync(pid);
		}

		private void GetFileFromDBCompleted (object sender, GetFileFromDBCompletedEventArgs e)
		{
			string[] files = e.Result.Split('*');		// 返回值的中文件名用*分隔

			FileList.Clear();
			foreach(string s in files)		
			{
				FileList.Add(s);
			}
		}

		/// <summary>
		/// 从数据库中读取目录，包括子目录
		/// </summary>
		/// <param name="path">目录地址</param>
		public void GetDirFromDB(string path)
		{
			webClient.GetDirFromDBCompleted += new EventHandler<GetDirFromDBCompletedEventArgs>(GetDirFromDBCompleted);

			DirList.Clear();

			// 把选择的目录加进去，以看到该目录下的文件
			DirList.Add(new DirInfoWithID(0, null, new DirectoryInfo(path)));	

			webClient.GetDirFromDBAsync(path);
		}

		private void GetDirFromDBCompleted(object sender, GetDirFromDBCompletedEventArgs e)
		{
			string[] dirStringArray = e.Result.Split('*');

			// silverlight不支持DataTable，所以用二维数组模拟DataTable
			string[][] dirs = new string[dirStringArray.Length-1][];

			for (int i = 0; i < dirStringArray.Length - 1; i++)
			{
				dirs[i] = dirStringArray[i].Split('|');
			}

			Queue<string[]> q = new Queue<string[]>();

			// 添加PID为NULL的目录到队列中，表示成几棵目录树的根节点
			for (int i = 0; i < dirs.Length; i++)		
			{
				if (dirs[i][1] == "NULL")				// PID为NULL
				{
					q.Enqueue(dirs[i]);
				}
			}

			// 先序遍历各棵目录树
			foreach(string[] i in q)
			{
				AddToDirList(i);

				GetAllChildren(dirs, i);
			}
			
			MessageBox.Show("载入完成，请再点击加载按钮");
		}

		/// <summary>
		/// Get所有孩子节点
		/// </summary>
		/// <param name="dirs">所有目录</param>
		/// <param name="i"></param>
		private void GetAllChildren(string[][] dirs, string[] i)
		{
			foreach (string[] j in dirs)
			{
				if (j[1] == i[0])			//j的PID等于i的ID
				{		
					AddToDirList(j);
					GetAllChildren(dirs, j);
				}
			}
		}

		private void AddToDirList(string[] dirString)
		{
			int id = Convert.ToInt32(dirString[0]);
			int? pid = null;
			try
			{
				pid = Convert.ToInt32(dirString[1]);
			}
			catch
			{
				pid = null;
			}
			DirectoryInfo info = new DirectoryInfo(dirString[2]);

			DirList.Add(new DirInfoWithID(id, pid, info));
		}
	}
}
