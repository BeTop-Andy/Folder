using System.Collections.Generic;
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

			SqlCommand sqlComm = new SqlCommand(string.Format(@"INSERT INTO FileTable([Id],[PID],[Name],[Size],[Type],[CreateTime],[Path])VALUES({0},{1},'{2}',{3},'{4}','{5}','{6}')", id, pid, file.Name, file.Length, file.Extension, file.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"), file.DirectoryName.Replace('\\', '/')), sqlConn);

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

			SqlCommand sqlComm = new SqlCommand(string.Format(@"INSERT INTO FileTable([Id],[PID],[Name],[Size],[Type],[CreateTime],[Path])VALUES({0},{1},'{2}',{3},'{4}','{5}','{6}')", id, pid, dir.Name, size, "dir", dir.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"), dir.Parent.FullName.Replace('\\', '/')), sqlConn);

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

		public string GetFileFromDB(int? PID)
		{
			sqlConn.Open();

			string sqlCommandString;

			if (PID.HasValue)
			{
				sqlCommandString = string.Format("SELECT * FROM FileTable WHERE [PID] = {0} AND [Type] like '.%'", PID);
			}
			else
			{
				sqlCommandString = string.Format("SELECT * FROM FileTable WHERE [PID] IS NULL AND [Type] like '.%'", PID);
			}

			SqlCommand sqlComm = new SqlCommand(sqlCommandString, sqlConn);
			SqlDataReader dr = sqlComm.ExecuteReader();

			string fileList = "";

			string fileName;
			while (dr.Read())
			{
				fileName = dr.GetString(3);

				fileList += fileName + "*";
			}

			dr.Close();

			return fileList;
		}

		public string GetDirFromDB(string path)
		{
			sqlConn.Open();

			if (path.EndsWith("/"))
			{
				path = path.Substring(0, path.Length - 1);
			}

			SqlCommand sqlComm = new SqlCommand(string.Format("SELECT * FROM FileTable WHERE [Path] LIKE '{0}%' AND [Type] = 'dir'", path), sqlConn);
			SqlDataReader dr = sqlComm.ExecuteReader();
			
			string dirList = "";

			int id;
			string pid;
			string fullName;
			while (dr.Read())
			{
				id = dr.GetInt32(1);
				pid = dr.IsDBNull(2) ? "NULL" : dr.GetInt32(2).ToString();
				fullName = dr.GetString(7) + "/" + dr.GetString(3);

				// 用“|”分隔元素
				// 用“*”分隔行
				dirList += string.Format("{0}|{1}|{2}", id, pid, fullName) + "*";
			}

			dr.Close();

			return dirList;
		}
	}
}
