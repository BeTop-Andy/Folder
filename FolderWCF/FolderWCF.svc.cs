using System.Collections.Generic;
using System.Web.Configuration;

using HuaweiSoftware.Folder.FolderDB;

namespace HuaweiSoftware.Folder.FolderWCF
{
	// 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“FolderWCF”。
	// 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 FolderWCF.svc 或 FolderWCF.svc.cs，然后开始调试。
	public class FolderWCF : IFolderWCF
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
		/// <returns>受影响的行数</returns>
		public int SaveData(List<List<string>> folders)
		{
			return DBHelper.SaveData(folders);
		}

		/// <summary>
		/// 从数据库中读取文件夹
		/// </summary>
		/// <returns>返回的文件夹LIST</returns>
		public List<List<string>> GetAllFolders()
		{
			return DBHelper.GetAllFolders();
		}

		/// <summary>
		/// 从数据库中读取文件
		/// </summary>
		/// <param name="PID">文件所在的文件夹的ID</param>
		/// <returns>返回的文件LIST</returns>
		public List<List<string>> GetFiles(int? PID = null)
		{
			return DBHelper.GetFiles(PID);
		}
	}
}
