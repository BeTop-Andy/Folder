using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace HuaweiSoftware.Folder.FolderDB
{
    public class DBHelper
	{
		/// <summary>
		/// 添加文件到数据库，插入之前先清空
		/// </summary>
		/// <param name="folders">两层的List.
		/// 最里面那层有type,id,pid,fullname.
		/// type:folder表示文件夹,file表示文件;
		/// id;
		/// pid;
		/// fullname:带路径的文件(夹)名</param>
		/// <param name="connectionString">数据库的连接字符串</param>
		/// <returns>受影响的行数</returns>
		public static int SaveData(List<List<string>> folders, string connectionString)
		{
			SqlConnection sqlConn = new SqlConnection(connectionString);

			sqlConn.Open();

			// 清空数据库
			SqlCommand sqlComm = new SqlCommand("DELETE FROM FileTable", sqlConn);

			sqlComm.ExecuteNonQuery();

			int rowCount = 0;

			string id;
			string pid;
			FileInfo file;
			DirectoryInfo dir;

			foreach (List<string> folder in folders)
			{
				id = folder[1];
				pid = folder[2];

				if (folder[0] == "file")		// 如果是文件
				{
					file = new FileInfo(folder[3]);

					sqlComm = new SqlCommand(string.Format(@"INSERT INTO FileTable([Id],[PID],[Name],[Size],[Type],[CreateTime])VALUES({0},{1},'{2}',{3},'{4}','{5}')", id, pid, file.Name, file.Length, file.Extension, file.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")), sqlConn);
				}
				else							// 否则就是文件夹
				{
					dir = new DirectoryInfo(folder[3]);

					long size = GetDirSize(dir);

					sqlComm = new SqlCommand(string.Format(@"INSERT INTO FileTable([Id],[PID],[Name],[Size],[Type],[CreateTime])VALUES({0},{1},'{2}',{3},'{4}','{5}')", id, pid, dir.Name, size, "dir", dir.CreationTime.ToString("yyyy-MM-dd HH:mm:ss")), sqlConn);
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
		private static long GetDirSize(DirectoryInfo dir)
		{
			long size = 0;

			FileInfo[] files = dir.GetFiles();
			foreach (var file in files)
			{
				size += file.Length;
			}

			DirectoryInfo[] dirs = dir.GetDirectories();
			foreach (var di in dirs)
			{
				size += GetDirSize(di);
			}

			return size;
		}

		/// <summary>
		/// 从数据库中读取文件夹
		/// </summary>
		/// <param name="connectionString">数据库的连接字符串</param>
		/// <returns>返回的文件夹LIST</returns>
		public static List<List<string>> GetAllFolders(string connectionString)
		{
			SqlConnection sqlConn = new SqlConnection(connectionString);

			sqlConn.Open();

			List<List<string>> folders = new List<List<string>>();

			SqlCommand sqlComm = new SqlCommand("SELECT * FROM FileTable WHERE [Type] = 'dir'", sqlConn);

			using (SqlDataReader dr = sqlComm.ExecuteReader())
			{
				int id;
				string pid;
				string name;
				List<string> dir;		// 临时变量
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

		/// <summary>
		/// 从数据库中读取文件
		/// </summary>
		/// <param name="PID">文件所在的文件夹的ID</param>
		/// <param name="connectionString">数据库的连接字符串</param>
		/// <returns>返回的文件LIST</returns>
		public static List<List<string>> GetFiles(string connectionString, int? PID = null)
		{
			SqlConnection sqlConn = new SqlConnection(connectionString);

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
				long size;
				string type;
				DateTime createTime;
				List<string> file;		// 临时变量
				while (dr.Read())
				{
					file = new List<string>();
					id = dr.GetInt32(1);
					pid = dr.IsDBNull(2) ? "NULL" : dr.GetInt32(2).ToString();
					name = dr.GetString(3);
					size = dr.GetInt64(4);
					type = dr.GetString(5);
					createTime = dr.GetDateTime(6);

					file.Add(id.ToString());
					file.Add(pid);
					file.Add(name);
					file.Add(size.ToString());
					file.Add(type);
					file.Add(createTime.ToString());

					files.Add(file);
				}
			}

			return files;
		}
    }
}
