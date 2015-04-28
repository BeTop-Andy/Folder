using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data.SqlClient;
using System.IO;

namespace HuaweiSoftware.Folder
{
	// 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“FolderWCF”。
	// 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 FolderWCF.svc 或 FolderWCF.svc.cs，然后开始调试。
	public class FolderWCF : IFolderWCF
	{
		private SqlConnection sqlConn = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Users\yesa\AppData\Local\Microsoft\VisualStudio\SSDT\filInfo.mdf;Integrated Security=True;Connect Timeout=30");

		/// <summary>
		/// 添加文件到数据库
		/// </summary>
		/// <param name="fileStr">文件信息字符串，格式“id|pid|fullname”</param>
		public void AddFileToDB(string fileStr)
		{
			sqlConn.Open();

			string[] temp = fileStr.Split('|');
			string id = temp[0];
			string pid = temp[1];
			FileInfo file = new FileInfo(temp[2]);

			SqlCommand sqlComm = new SqlCommand(string.Format(@"INSERT INTO FileTable([Id],[PID],[Name],[Size],[Type],[CreateTime],[Path])VALUES({0},{1},'{2}',{3},'{4}','{5}','{6}')", id, pid, file.Name, file.Length, file.Extension, file.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"), file.DirectoryName), sqlConn);

			sqlComm.ExecuteNonQuery();
		}

		/// <summary>
		/// 添加目录到数据库
		/// </summary>
		/// <param name="dirStr">目录信息字符串，格式“id|pid|fullname”</param>
		public void AddDirToDB(string dirStr)
		{
			sqlConn.Open();

			string[] temp = dirStr.Split('|');
			string id = temp[0];
			string pid = temp[1];
			DirectoryInfo dir = new DirectoryInfo(temp[2]);


			long size = GetDirSize(dir);

			SqlCommand sqlComm = new SqlCommand(string.Format(@"INSERT INTO FileTable([Id],[PID],[Name],[Size],[Type],[CreateTime],[Path])VALUES({0},{1},'{2}',{3},'{4}','{5}','{6}')", id, pid, dir.Name, size, "dir", dir.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"), dir.Parent.FullName), sqlConn);

			sqlComm.ExecuteNonQuery();

		}

		/// <summary>
		/// 计算dir目录下（包括子目录）的所有文件的总大小
		/// </summary>
		/// <param name="dir"></param>
		/// <returns>大小</returns>
		private long GetDirSize(DirectoryInfo dir)
		{
			long size = 0;

			IEnumerable<FileInfo> files = dir.EnumerateFiles();
			foreach (var file in files)
			{
				size += file.Length;
			}

			IEnumerable<DirectoryInfo> dirs = dir.EnumerateDirectories();
			foreach (var di in dirs)
			{
				size += GetDirSize(di);
			}

			return size;
		}

		public bool Exists(string path, string name)
		{
			sqlConn.Open();

			SqlCommand sqlComm = new SqlCommand(string.Format("SELECT * FROM FileTable WHERE [Path] = '{0}' AND [Name] = '{1}'", path, name), sqlConn);

			SqlDataReader dr = sqlComm.ExecuteReader();

			bool isExists = dr.HasRows;

			dr.Close();

			return isExists;
		}

		public int GetId()
		{
			sqlConn.Open();

			SqlCommand sqlComm = new SqlCommand("SELECT MAX(Id) FROM FileTable", sqlConn);
			SqlDataReader dr = sqlComm.ExecuteReader();

			int id;

			if (dr.Read())
			{
				id = dr.GetInt32(0);
			}
			else
			{
				id = 1;
			}

			dr.Close();

			return id;
		}

		public List<string> GetFileFromDB(string path)
		{
			sqlConn.Open();

			if (path.EndsWith(@"\"))
			{
				path = path.Substring(0, path.Length - 1);
			}

			SqlCommand sqlComm = new SqlCommand(string.Format("SELECT * FROM FileTable WHERE [Path] LIKE '{0}%' AND [Type] <> 'dir' ORDER BY [Id]", path), sqlConn);
			SqlDataReader dr = sqlComm.ExecuteReader();

			List<string> fileList = new List<string>();

			int id;
			int? pid;
			string fullName;
			while (dr.Read())
			{
				id = dr.GetInt32(1);
				pid = dr.GetInt32(2);
				fullName = dr.GetString(7) + @"\" + dr.GetString(3);

				fileList.Add(string.Format("{0}|{1}|{2}", id, pid, fullName));
			}

			dr.Close();

			return fileList;
		}

		public string GetDirFromDB(string path)
		{
			sqlConn.Open();

			if (path.EndsWith(@"\"))
			{
				path = path.Substring(0, path.Length - 1);
			}

			SqlCommand sqlComm = new SqlCommand(string.Format("SELECT * FROM FileTable WHERE [Path] LIKE '{0}%' AND [Type] = 'dir' ORDER BY [Id]", path), sqlConn);
			SqlDataReader dr = sqlComm.ExecuteReader();

			string dirList = "";

			int id;
			int? pid;
			string fullName;
			while (dr.Read())
			{
				id = dr.GetInt32(1);
				pid = dr.GetInt32(2);
				fullName = dr.GetString(7) + @"\" + dr.GetString(3);

				dirList += string.Format("{0}|{1}|{2}", id, pid, fullName) + "*";
			}

			dr.Close();

			return dirList;
		}
	}
}
