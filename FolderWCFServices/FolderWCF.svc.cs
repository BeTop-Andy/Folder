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
		/// 添加文件到数据库，插入之前先清空
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

			//清空数据库
			SqlCommand sqlComm = new SqlCommand("DELETE FROM FileTable", sqlConn);

			sqlComm.ExecuteNonQuery();

			int rowCount = 0;

			string id;
			string pid;
			FileInfo file;
			DirectoryInfo dir;

			foreach (List<string> l in folders)
			{
				id = l[1];
				pid = l[2];

				if (l[0] == "file")
				{
					file = new FileInfo(l[3]);

					sqlComm = new SqlCommand(string.Format(@"INSERT INTO FileTable([Id],[PID],[Name],[Size],[Type],[CreateTime])VALUES({0},{1},'{2}',{3},'{4}','{5}')", id, pid, file.Name, file.Length, file.Extension, file.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")), sqlConn);
				}
				else if (l[0] == "folder")
				{
					dir = new DirectoryInfo(l[3]);

					long size = GetDirSize(dir);

					sqlComm = new SqlCommand(string.Format(@"INSERT INTO FileTable([Id],[PID],[Name],[Size],[Type],[CreateTime])VALUES({0},{1},'{2}',{3},'{4}','{5}')", id, pid, dir.Name, size, "dir", dir.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")), sqlConn);
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

		public List<List<string>> GetDirListFromDB()
		{
			sqlConn.Open();

			List<List<string>> folders = new List<List<string>>();

			SqlCommand sqlComm = new SqlCommand("SELECT * FROM FileTable WHERE [Type] = 'dir'", sqlConn);

			using (SqlDataReader dr = sqlComm.ExecuteReader())
			{
				int id;
				string pid;
				string name;
				List<string> dir;		//临时变量
				while (dr.Read())
				{
					dir = new List<string>();
					id = dr.GetInt32(1);
					pid = dr.IsDBNull(2) ? "NULL" : dr.GetInt32(2).ToString();
					name = dr.GetString(3);

					dir.Add(id.ToString());
					dir.Add(pid);
					dir.Add(name);

					folders.Add(dir);
				}
			}

			return folders;
		}

		public List<List<string>> GetFileListFromDB(int? PID = null)
		{
			sqlConn.Open();

			List<List<string>> files = new List<List<string>>();

			string sqlCommandString = "SELECT * FROM FileTable";

			if (PID.HasValue)
			{
				sqlCommandString = string.Format("SELECT * FROM FileTable WHERE [PID] = {0} AND [Type] LIKE '.%'", PID);
			}
			else
			{
				sqlCommandString = "SELECT * FROM FileTable WHERE [PID] IS NULL AND [Type] LIKE '.%'";
			}

			SqlCommand sqlComm = new SqlCommand(sqlCommandString, sqlConn);
			using (SqlDataReader dr = sqlComm.ExecuteReader())
			{
				int id;
				string pid;
				string name;
				List<string> file;		//临时变量
				while (dr.Read())
				{
					file = new List<string>();
					id = dr.GetInt32(1);
					pid = dr.IsDBNull(2) ? "NULL" : dr.GetInt32(2).ToString();
					name = dr.GetString(3);

					file.Add(id.ToString());
					file.Add(pid);
					file.Add(name);

					files.Add(file);
				}
			}

			return files;
		}
	}
}
