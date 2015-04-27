using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data.SqlClient;

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
		/// <param name="id"></param>
		/// <param name="pid"></param>
		/// <param name="fileName"></param>
		/// <param name="size"></param>
		/// <param name="ext">后缀名</param>
		/// <param name="createTime"></param>
		public void AddFileToDB(int id, int? pid, string fileName, long size, string ext, DateTime createTime)
		{
			sqlConn.Open();
			SqlCommand sqlComm = new SqlCommand(string.Format(@"INSERT INTO FileTable([Id],[PID],[Name],[Size],[Type],[createTime])VALUES({0},{1},'{2}',{3},'{4}','{5}')", id, pid.HasValue ? pid.Value.ToString() : "NULL", fileName, size, ext, createTime.ToString("yyyy-MM-dd HH:mm:ss")), sqlConn);
			sqlComm.ExecuteNonQuery();
		}
	}
}
