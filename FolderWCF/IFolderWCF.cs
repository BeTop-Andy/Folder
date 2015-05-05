using System.Collections.Generic;
using System.ServiceModel;

namespace HuaweiSoftware.Folder.FolderWCF
{
	// 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IFolderWCF”。
	[ServiceContract]
	public interface IFolderWCF
	{
		[OperationContract]
		int SaveData(List<List<string>> folders);

		[OperationContract]
		List<List<string>> GetAllFolders();

		[OperationContract]
		List<List<string>> GetFiles(int? PID);
	}
}
