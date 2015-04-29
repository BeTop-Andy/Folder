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

		private ObservableCollection<ObservableCollection<string>> folders;

		public DataBaseOperator()
		{
			webClient = new FolderWCFClient();

			webClient.GetIdCompleted += ((sender, e) => id = e.Result + 1);
			webClient.GetIdAsync();

			FileList = new ObservableCollection<string>();
			DirList = new ObservableCollection<DirInfoWithID>();
			folders = new ObservableCollection<ObservableCollection<string>>();
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
		/// 获取dir目录下的所有子目录，并加入List
		/// </summary>
		/// <param name="dir">“根”目录</param>
		/// <param name="pid">父节点的Id</param>
		public void AddDirToList(DirectoryInfo dir, int? pid)
		{
			IEnumerable<DirectoryInfo> dirs = dir.EnumerateDirectories();

			ObservableCollection<string> folder;
			foreach (DirectoryInfo di in dirs)
			{
				int tmp_id = id;

				//if (!CheckExists(di.Parent.FullName, di.Name))
				//{
				//MessageBox.Show(id.ToString());
				folder = new ObservableCollection<string>();
				folder.Add("folder");
				folder.Add(id.ToString());
				folder.Add(pid.HasValue ? pid.Value.ToString() : "NULL");
				folder.Add(di.FullName);
				/*foreach (var file in folders)
				{
					MessageBox.Show("preFolders: " + file[0] + " | " + file[1] + " | " + file[2] + " | " + file[3]);
				}
				MessageBox.Show("Folder: " + folder[0] + " | " + folder[1] + " | " + folder[2] + " | " + folder[3]);*/
				folders.Add(folder);
				/*foreach (var file in folders)
				{
					MessageBox.Show("Folders: " + file[0] + " | " + file[1] + " | " + file[2] + " | " + file[3]);
				}*/

				id++;
				//}

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
			ObservableCollection<string> file;

			foreach (FileInfo fi in files)
			{
				//if (!CheckExists(fi.DirectoryName, fi.Name))
				//{
				//MessageBox.Show(id.ToString());
				file = new ObservableCollection<string>();
				file.Add("file");
				file.Add(id.ToString());
				file.Add(pid.HasValue ? pid.Value.ToString() : "NULL");
				file.Add(fi.FullName);
				folders.Add(file);
				//MessageBox.Show("id:" + file[1]);

				id++;
				//}
			}
		}

		public void AddListToDB()
		{
			MessageBox.Show(folders.Count.ToString());
			webClient.AddListToDBCompleted -= ((sender, e) =>
			{
				MessageBox.Show("成功插入" + e.Result + "行\n保存完毕");
			});
			webClient.AddListToDBCompleted += ((sender, e) =>
			{
				MessageBox.Show("成功插入" + e.Result + "行\n保存完毕");
			});

			/*foreach (var file in folders)
			{
				MessageBox.Show(file[0] + " | " + file[1] + " | " + file[2] + " | " + file[3]);
			}*/
			webClient.AddListToDBAsync(folders);
		}

		public void ClearDBList()
		{
			folders.Clear();
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

		private void GetFileFromDBCompleted(object sender, GetFileFromDBCompletedEventArgs e)
		{
			string[] files = e.Result.Split('*');		// 返回值的中文件名用*分隔

			FileList.Clear();
			foreach (string s in files)
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
			string[][] dirs = new string[dirStringArray.Length - 1][];

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
			foreach (string[] i in q)
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
