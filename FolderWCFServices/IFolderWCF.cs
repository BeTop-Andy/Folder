using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data.SqlClient;

namespace HuaweiSoftware.Folder
{
	// 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IFolderWCF”。
	[ServiceContract]
	public interface IFolderWCF
	{
		[OperationContract]
		int AddListToDB(List<List<string>> folders);

		[OperationContract]
		bool Exists(string path, string name);

		[OperationContract]
		int GetId();

		[OperationContract]
		List<List<string>> GetDirListFromDB(string path);

		[OperationContract]
		List<List<string>> GetFileListFromDB(string path, int? PID);
	}
}
