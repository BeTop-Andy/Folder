using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using HuaweiSoftware.Folder.FolderUI.FolderWCFReference;

namespace HuaweiSoftware.Folder.FolderUI
{
	public class FolderHelper
	{
		static private FolderWCFClient m_WebClient;

		static private int m_ID;		// 每一个文件（夹）标识

		// 根目录
		private List<string> m_Root;

		// 存放从本地读到的数据
		private List<List<string>> m_Folders;

		// 存放从数据库读到的文件数据
		private List<List<string>> m_FileList;
		public List<List<string>> FileList
		{
			get
			{
				return m_FileList;
			}
			set
			{
				m_FileList = value;
			}
		}

		// 存放从数据库读到的文件夹数据
		private List<List<string>> m_DirList;
		public List<List<string>> DirList
		{
			get
			{
				return m_DirList;
			}
			set
			{
				m_DirList = value;
			}
		}

		public event EventHandler onLoadDirFinish;		// 触发读取目录完成事件
		public event EventHandler onLoadFileFinish;		// 触发读取文件完成事件

		public FolderHelper()
		{
			m_WebClient = new FolderWCFClient();

			m_ID = 1;

			m_FileList = new List<List<string>>();
			m_DirList = new List<List<string>>();
			m_Folders = new List<List<string>>();

			m_Root = new List<string>();

			// 根目录的相关属性
			m_Root.Add("0");		// 保存树节点（文件夹）的ID
			m_Root.Add("");			// PID，根目录所以为空
			m_Root.Add("Root");		// 名称
		}

		/// <summary>
		/// 获取dir目录下的所有子目录，并加入List
		/// </summary>
		/// <param name="dir">“根”目录</param>
		/// <param name="pid">父节点的Id</param>
		public void AddDirToList(DirectoryInfo dir, int? pid)
		{
			IEnumerable<DirectoryInfo> dirs = dir.EnumerateDirectories();

			List<string> tmp_folder;
			foreach (DirectoryInfo di in dirs)
			{
				int tmp_id = m_ID;		// 临时保存ID

				tmp_folder = new List<string>();		// 临时变量
				tmp_folder.Add("folder");
				tmp_folder.Add(m_ID.ToString());
				tmp_folder.Add(pid.HasValue ? pid.Value.ToString() : "NULL");
				tmp_folder.Add(di.FullName);

				m_Folders.Add(tmp_folder);

				m_ID++;

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

			List<string> tmp_file;		// 临时变量

			foreach (FileInfo file in files)
			{
				tmp_file = new List<string>();		
				tmp_file.Add("file");
				tmp_file.Add(m_ID.ToString());
				tmp_file.Add(pid.HasValue ? pid.Value.ToString() : "NULL");
				tmp_file.Add(file.FullName);
				m_Folders.Add(tmp_file);

				m_ID++;
			}
		}

		/// <summary>
		/// 上传到数据库
		/// </summary>
		public void SavaData()
		{
			m_WebClient.SaveDataCompleted -= new EventHandler<SaveDataCompletedEventArgs>(SaveDataCompleted);
			m_WebClient.SaveDataCompleted += new EventHandler<SaveDataCompletedEventArgs>(SaveDataCompleted);

			m_WebClient.SaveDataAsync(m_Folders);
		}

		private void SaveDataCompleted(object sender, SaveDataCompletedEventArgs e)
		{
			MessageBox.Show("成功插入" + e.Result + "行\n保存完毕");
		}

		public void ClearDBList()
		{
			m_Folders.Clear();
		}

		/// <summary>
		/// 从数据库中读取文件
		/// </summary>
		/// <param name="id">目录ID</param>
		public void GetFiles(int id)
		{
			m_WebClient.GetFilesCompleted -= new EventHandler<GetFilesCompletedEventArgs>(GetFilesCompleted);
			m_WebClient.GetFilesCompleted += new EventHandler<GetFilesCompletedEventArgs>(GetFilesCompleted);

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

			m_WebClient.GetFilesAsync(pid);
		}

		/// <summary>
		/// 读取完成，转存到fileList中
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void GetFilesCompleted(object sender, GetFilesCompletedEventArgs e)
		{
			List<List<string>> files = e.Result;
			List<string> tmp_file;	// 临时变量

			m_FileList.Clear();

			foreach (var file in files)
			{
				tmp_file = new List<string>();

				// 排序按windows资源管理器来
				tmp_file.Add(file[2]);	// 名称
				tmp_file.Add(file[5]);	// 创建日期
				tmp_file.Add(file[4]);	// 类型
				// 文件大小,转成KB
				tmp_file.Add(ConvertToKB(Convert.ToInt64(file[3])));

				m_FileList.Add(tmp_file);
			}

			onLoadFileFinish(null, null);
		}

		/// <summary>
		/// 字节转换成KB
		/// </summary>
		/// <param name="num">原来的数值</param>
		/// <returns>转换后的数值</returns>
		private string ConvertToKB(long num)
		{
			long result = num >> 10;	// 除以1024

			// 有余数，进一
			if (num % 1024 != 0)
			{
				result++;
			}

			return result.ToString()+ " KB";
		}

		/// <summary>
		/// 从数据库中读取目录，包括子目录
		/// </summary>
		/// <param name="path">目录地址</param>
		public void GetAllFolders()
		{
			m_WebClient.GetAllFoldersCompleted += new EventHandler<GetAllFoldersCompletedEventArgs>(GetAllFoldersCompleted);

			m_DirList.Clear();

			// 把选择的目录加进去，以看到该目录下的文件
			m_DirList.Add(m_Root);

			m_WebClient.GetAllFoldersAsync();
		}

		private void GetAllFoldersCompleted(object sender, GetAllFoldersCompletedEventArgs e)
		{
			foreach (List<string> dir in e.Result)
			{
				m_DirList.Add(dir);
			}

			onLoadDirFinish(null, null);
		}
	}
}
