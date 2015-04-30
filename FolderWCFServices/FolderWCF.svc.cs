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
		/// <param name="folders">两层的List.
		/// 最里面那层有type,id,pid,fullname.
		/// type:folder表示文件夹,file表示文件;
		/// id;
		/// pid;
		/// fullname:带路径的文件(夹)名</param>
		/// <returns>受影响的行数</returns>
		public int AddListToDB(List<List<string>> folders)
		{
			sqlConn.Open();
			int rowCount = 0;

			string id;
			string pid;
			FileInfo file;
			DirectoryInfo dir;
			SqlCommand sqlComm = new SqlCommand();
			foreach (List<string> l in folders)
			{
				id = l[1];
				pid = l[2];

				if (l[0] == "file")
				{
					file = new FileInfo(l[3]);

					sqlComm = new SqlCommand(string.Format(@"INSERT INTO FileTable([Id],[PID],[Name],[Size],[Type],[CreateTime],[Path])VALUES({0},{1},'{2}',{3},'{4}','{5}','{6}')", id, pid, file.Name, file.Length, file.Extension, file.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"), file.DirectoryName.Replace('\\', '/')), sqlConn);
				}
				else if (l[0] == "folder")
				{
					dir = new DirectoryInfo(l[3]);

					long size = GetDirSize(dir);

					sqlComm = new SqlCommand(string.Format(@"INSERT INTO FileTable([Id],[PID],[Name],[Size],[Type],[CreateTime],[Path])VALUES({0},{1},'{2}',{3},'{4}','{5}','{6}')", id, pid, dir.Name, size, "dir", dir.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"), dir.Parent.FullName.Replace('\\', '/')), sqlConn);
				}
				else
				{
					return 0;
				}

				rowCount += sqlComm.ExecuteNonQuery();
			}

			return rowCount;
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
			int id;

			sqlConn.Open();

			SqlCommand sqlComm = new SqlCommand("SELECT MAX(Id) FROM FileTable", sqlConn);
			using (SqlDataReader dr = sqlComm.ExecuteReader())
			{
				if (dr.Read())
				{
					id = dr.GetInt32(0);
				}
				else
				{
					id = 1;
				}
			}

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

		public List<List<string>> GetDirListFromDB(string path)
		{
			sqlConn.Open();

			List<List<string>> folders = new List<List<string>>();

			path = path.Replace('\\', '/');

			// 剪去如同“D:/test/”中的最后那个"/"。
			if (path.EndsWith("/"))
			{
				path = path.Substring(0, path.Length - 1);
			}

			SqlCommand sqlComm = new SqlCommand(string.Format("SELECT * FROM FileTable WHERE [Path] LIKE '{0}%' AND [Type] = 'dir'", path), sqlConn);
			using (SqlDataReader dr = sqlComm.ExecuteReader())
			{
				int id;
				string pid;
				string fullName;
				List<string> dir;		//临时变量
				while (dr.Read())
				{
					dir = new List<string>();
					id = dr.GetInt32(1);
					pid = dr.IsDBNull(2) ? "NULL" : dr.GetInt32(2).ToString();
					fullName = dr.GetString(7) + "/" + dr.GetString(3);

					dir.Add(id.ToString());
					dir.Add(pid);
					dir.Add(fullName);

					folders.Add(dir);
				}
			}

			return folders;
		}

		public List<List<string>> GetFileListFromDB(string path, int? PID = null)
		{
			sqlConn.Open();

			List<List<string>> files = new List<List<string>>();

			path = path.Replace('\\', '/');

			// 剪去如同“D:/test/”中的最后那个"/"。
			if (path.EndsWith("/"))
			{
				path = path.Substring(0, path.Length - 1);
			}

			string sqlCommandString = "SELECT * FROM FileTable";

			if (PID.HasValue)
			{
				sqlCommandString = string.Format("SELECT * FROM FileTable WHERE [PID] = {0} AND [Type] LIKE '.%'", PID);
			}
			else
			{
				sqlCommandString = string.Format("SELECT * FROM FileTable WHERE [Path] = '{0}' AND [Type] LIKE '.%'", path);
			}

			SqlCommand sqlComm = new SqlCommand(sqlCommandString, sqlConn);
			using (SqlDataReader dr = sqlComm.ExecuteReader())
			{
				int id;
				string pid;
				string fullName;
				List<string> file;		//临时变量
				while (dr.Read())
				{
					file = new List<string>();
					id = dr.GetInt32(1);
					pid = dr.IsDBNull(2) ? "NULL" : dr.GetInt32(2).ToString();
					fullName = dr.GetString(7) + "/" + dr.GetString(3);

					file.Add(id.ToString());
					file.Add(pid);
					file.Add(fullName);

					files.Add(file);
				}
			}

			return files;
		}
	}
}
