using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using HuaweiSoftware.Folder.FolderUI.FolderWCFReference;

namespace HuaweiSoftware.Folder.FolderUI
{
	public class DataBaseOperator
	{
		static private FolderWCFClient webClient;

		static private int id;		// 每一个文件（夹）标识

		// 根目录
		private TreeViewItem root;

		// 存放从数据库中读取的数据
		private List<List<string>> folders;

		// 存放处理后的数据
		public ObservableCollection<File> FileList
		{
			get;
			set;
		}

		// 存放处理后的数据
		public ObservableCollection<TreeViewItem> DirTree
		{
			get;
			set;
		}

		public event EventHandler onLoadDirFinish;		// 触发读取目录完成事件
		public event EventHandler onLoadFileFinish;		// 触发读取文件完成事件

		public DataBaseOperator()
		{
			webClient = new FolderWCFClient();

			id = 1;

			FileList = new ObservableCollection<File>();
			DirTree = new ObservableCollection<TreeViewItem>();
			folders = new List<List<string>>();
			root = new TreeViewItem();

			// 根目录的相关属性
			root.Header = "Root";
			root.Tag = 0;		// 保存树节点（文件夹）的ID
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

				folder = new List<string>();		// 临时变量
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
				file = new List<string>();		// 临时变量
				file.Add("file");
				file.Add(id.ToString());
				file.Add(pid.HasValue ? pid.Value.ToString() : "NULL");
				file.Add(fi.FullName);
				folders.Add(file);

				id++;
			}
		}

		/// <summary>
		/// 上传到数据库
		/// </summary>
		public void SavaData()
		{
			webClient.SaveDataCompleted -= new EventHandler<SaveDataCompletedEventArgs>(SaveDataCompleted);
			webClient.SaveDataCompleted += new EventHandler<SaveDataCompletedEventArgs>(SaveDataCompleted);

			webClient.SaveDataAsync(folders);
		}

		private void SaveDataCompleted(object sender, SaveDataCompletedEventArgs e)
		{
			MessageBox.Show("成功插入" + e.Result + "行\n保存完毕");
		}

		public void ClearDBList()
		{
			folders.Clear();
		}

		/// <summary>
		/// 从数据库中读取文件
		/// </summary>
		/// <param name="id">目录ID</param>
		public void GetFiles(int id)
		{
			webClient.GetFilesCompleted -= new EventHandler<GetFilesCompletedEventArgs>(GetFilesCompleted);
			webClient.GetFilesCompleted += new EventHandler<GetFilesCompletedEventArgs>(GetFilesCompleted);

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

			webClient.GetFilesAsync(pid);
		}

		/// <summary>
		/// 读取完成，转存到fileList中
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GetFilesCompleted(object sender, GetFilesCompletedEventArgs e)
		{
			List<List<string>> files = e.Result;
			File fi;	// 临时变量

			FileList.Clear();

			foreach (var file in files)
			{
				fi = new File
				{
					Name = file[2],
					// 转换成KB
					Size = ConvertToKB(Convert.ToInt64(file[3])),
					Type = file[4],
					CreateTime = file[5]
				};
				FileList.Add(fi);
			}

			onLoadFileFinish(null, null);
		}

		/// <summary>
		/// 字节转换成KB
		/// </summary>
		/// <param name="num">原来的数值</param>
		/// <returns>转换后的数值</returns>
		private long ConvertToKB(long num)
		{
			long result = num >> 10;	// 除以1024

			// 有余数，进一
			if (num % 1024 != 0)
			{
				result++;
			}

			return result;
		}

		/// <summary>
		/// 从数据库中读取目录，包括子目录
		/// </summary>
		/// <param name="path">目录地址</param>
		public void GetAllFolders()
		{
			webClient.GetAllFoldersCompleted += new EventHandler<GetAllFoldersCompletedEventArgs>(GetAllFoldersCompleted);

			DirTree.Clear();

			// 把选择的目录加进去，以看到该目录下的文件
			DirTree.Add(root);

			webClient.GetAllFoldersAsync();
		}

		private void GetAllFoldersCompleted(object sender, GetAllFoldersCompletedEventArgs e)
		{
			folders = e.Result;

			// 用于存放PID为NULL的目录，相当于几棵目录树的根节点，所以叫treeRoots
			List<List<string>> treeRoots = new List<List<string>>();

			// 找PID为NULL的目录
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
				AddToDirTree(treeRoot, 1, root);
			}

			onLoadDirFinish(null, null);
		}

		/// <summary>
		/// 把目录加入目录树
		/// </summary>
		/// <param name="dir">目录相关信息</param>
		/// <param name="level">深度</param>
		/// <param name="parentNode">父节点</param>
		private void AddToDirTree(List<string> dir, int level, TreeViewItem parentNode)
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

			TreeViewItem childNode = new TreeViewItem();
			childNode.Header = name;
			childNode.Tag = id;		// id

			if (parentNode != null)
			{
				parentNode.Items.Add(childNode);
			}
			else
			{
				DirTree.Add(childNode);
			}

			GetAllChildren(dir, level + 1, childNode);
		}

		/// <summary>
		/// Get所有孩子节点
		/// </summary>
		/// <param name="nowDir"></param>
		/// <param name="level">深度</param>
		/// <param name="nowNode">现在的节点</param>
		private void GetAllChildren(List<string> nowDir, int level, TreeViewItem nowNode)
		{
			foreach (List<string> tempDir in folders)
			{
				// 找“孩子”
				// tempDir的PID等于nowDir的ID
				if (tempDir[1] == nowDir[0])
				{
					AddToDirTree(tempDir, level, nowNode);
				}
			}
		}
	}
}
