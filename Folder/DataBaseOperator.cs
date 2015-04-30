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

		private List<List<string>> folders;

		public DataBaseOperator()
		{
			webClient = new FolderWCFClient();

			//webClient.GetIdCompleted += ((sender, e) => id = e.Result + 1);
			//webClient.GetIdAsync();

			id = 1;

			FileList = new ObservableCollection<FileInfo>();
			DirList = new ObservableCollection<DirInfoWithID>();
			folders = new List<List<string>>();
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

			List<string> folder;
			foreach (DirectoryInfo di in dirs)
			{
				int tmp_id = id;

				//if (!CheckExists(di.Parent.FullName, di.Name))
				//{
				//MessageBox.Show(id.ToString());
				folder = new List<string>();
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
			List<string> file;

			foreach (FileInfo fi in files)
			{
				//if (!CheckExists(fi.DirectoryName, fi.Name))
				//{
				//MessageBox.Show(id.ToString());
				file = new List<string>();
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

		public void GetFileListFromDB(string path, int id)
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

			webClient.GetFileListFromDBAsync(path, pid);
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
		public void GetDirFromDB(string path)
		{
			webClient.GetDirListFromDBCompleted += new EventHandler<GetDirListFromDBCompletedEventArgs>(GetDirListFromDBCompleted);

			DirList.Clear();

			// 把选择的目录加进去，以看到该目录下的文件
			DirList.Add(new DirInfoWithID(0, null, new DirectoryInfo(path)));

			webClient.GetDirListFromDBAsync(path);
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
				AddToDirList(treeRoot);

				GetAllChildren(folders, treeRoot);
			}

			MessageBox.Show("载入完成，请再点击加载按钮");
		}

		private void AddToDirList(List<string> dir)
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
			DirectoryInfo info = new DirectoryInfo(dir[2]);

			DirList.Add(new DirInfoWithID(id, pid, info));
		}

		/// <summary>
		/// Get所有孩子节点
		/// </summary>
		/// <param name="dirs">所有目录</param>
		/// <param name="dir"></param>
		private void GetAllChildren(List<List<string>> dirs, List<string> dir)
		{
			foreach (List<string> nowDir in dirs)
			{
				if (nowDir[1] == dir[0])			//nowDir的PID等于dir的ID
				{
					AddToDirList(nowDir);

					GetAllChildren(dirs, nowDir);
				}
			}
		}
	}
}
