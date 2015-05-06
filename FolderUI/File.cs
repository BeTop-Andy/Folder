namespace HuaweiSoftware.Folder.FolderUI
{
	// 用来储存文件的相关信息
	public class File
	{
		// 文件名
		public string Name	
		{
			get;
			set;
		}

		// 文件大小
		public long Size
		{
			get;
			set;
		}

		// 创建时间
		public string CreateTime
		{
			get;
			set;
		}

		// 文件类型（即后缀名）
		public string Type
		{
			get;
			set;
		}
	}
}
