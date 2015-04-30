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

		public ObservableCollection<FileInfo> FileList
		{
			get;
			set;
		}

		public ObservableCollection<DirNameWithID> DirList
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

		private List<List<string>> folders;

		public DataBaseOperator()
		{
			webClient = new FolderWCFClient();

			id = 1;

			FileList = new ObservableCollection<FileInfo>();
			DirList = new ObservableCollection<DirNameWithID>();
			folders = new List<List<string>>();
		}

		/// <summary>
		/// 获取dir目录下的所有子目录，并加入List
		/// </summary>
		/// <param name="dir">“根”目录</param>
		/// <param name="pid">父节点的Id</param>
		public void AddDirToList(DirectoryInfo dir, int? pid)
		{
			IEnumerable<DirectoryInfo> dirs = dir.EnumerateDirectories();

			List<string> folder;
			foreach (DirectoryInfo di in dirs)
			{
				int tmp_id = id;

				folder = new List<string>();
				folder.Add("folder");
				folder.Add(id.ToString());
				folder.Add(pid.HasValue ? pid.Value.ToString() : "NULL");
				folder.Add(di.FullName);

				folders.Add(folder);

				id++;

				AddFileToList(di, tmp_id);
				AddDirToList(di, tmp_id);
			}
		}

		/// <summary>
		/// 获取dir目录下的所有文件，并加入List
		/// </summary>
		/// <param name="dir">“根”目录</param>
		/// <param name="pid">父节点的Id</param>
		public void AddFileToList(DirectoryInfo dir, int? pid)
		{
			IEnumerable<FileInfo> files = dir.EnumerateFiles();
			List<string> file;

			foreach (FileInfo fi in files)
			{
				file = new List<string>();
				file.Add("file");
				file.Add(id.ToString());
				file.Add(pid.HasValue ? pid.Value.ToString() : "NULL");
				file.Add(fi.FullName);
				folders.Add(file);

				id++;
			}
		}

		public void AddListToDB()
		{
			webClient.AddListToDBCompleted -= new EventHandler<AddListToDBCompletedEventArgs>(AddListToDBCompleted);
			webClient.AddListToDBCompleted += new EventHandler<AddListToDBCompletedEventArgs>(AddListToDBCompleted);

			webClient.AddListToDBAsync(folders);
		}

		private void AddListToDBCompleted(object sender, AddListToDBCompletedEventArgs e)
		{
			MessageBox.Show("成功插入" + e.Result + "行\n保存完毕");
		}

		public void ClearDBList()
		{
			folders.Clear();
		}

		public void GetFileListFromDB(int id)
		{
			webClient.GetFileListFromDBCompleted -= new EventHandler<GetFileListFromDBCompletedEventArgs>(GetFileListFromDBCompleted);
			webClient.GetFileListFromDBCompleted += new EventHandler<GetFileListFromDBCompletedEventArgs>(GetFileListFromDBCompleted);

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

			webClient.GetFileListFromDBAsync(pid);
		}

		private void GetFileListFromDBCompleted(object sender, GetFileListFromDBCompletedEventArgs e)
		{
			List<List<string>> files = e.Result;
			FileInfo fi;

			FileList.Clear();

			foreach (var file in files)
			{
				fi = new FileInfo(file[2]);
				FileList.Add(fi);
			}
		}

		/// <summary>
		/// 从数据库中读取目录，包括子目录
		/// </summary>
		/// <param name="path">目录地址</param>
		public void GetDirFromDB()
		{
			webClient.GetDirListFromDBCompleted += new EventHandler<GetDirListFromDBCompletedEventArgs>(GetDirListFromDBCompleted);

			DirList.Clear();

			// 把选择的目录加进去，以看到该目录下的文件
			DirList.Add(new DirNameWithID(0, null, "."));

			webClient.GetDirListFromDBAsync();
		}

		private void GetDirListFromDBCompleted(object sender, GetDirListFromDBCompletedEventArgs e)
		{
			List<List<string>> folders = e.Result;

			// 用于存放PID为NULL的目录，相当于几棵目录树的根节点，所以叫treeRoots
			List<List<string>> treeRoots = new List<List<string>>();

			foreach (var dir in folders)
			{
				if (dir[1] == "NULL")
				{
					treeRoots.Add(dir);
				}
			}

			// 先序遍历各棵目录树
			foreach (var treeRoot in treeRoots)
			{
				AddToDirList(treeRoot, 1);

				GetAllChildren(folders, treeRoot, 2);
			}

			MessageBox.Show("载入完成，请再点击加载按钮");
		}

		private void AddToDirList(List<string> dir, int level)
		{
			int id = Convert.ToInt32(dir[0]);
			int? pid = null;
			try
			{
				pid = Convert.ToInt32(dir[1]);
			}
			catch
			{
				pid = null;
			}
			string name = dir[2];

			DirList.Add(new DirNameWithID(id, pid, name, level));
		}

		/// <summary>
		/// Get所有孩子节点
		/// </summary>
		/// <param name="dirs">所有目录</param>
		/// <param name="dir"></param>
		/// <param name="level">深度</param>
		private void GetAllChildren(List<List<string>> dirs, List<string> dir, int level)
		{
			foreach (List<string> nowDir in dirs)
			{
				if (nowDir[1] == dir[0])			//nowDir的PID等于dir的ID
				{
					AddToDirList(nowDir, level);

					GetAllChildren(dirs, nowDir, level + 1);
				}
			}
		}
	}
}
