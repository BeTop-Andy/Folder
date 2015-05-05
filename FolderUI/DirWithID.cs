namespace HuaweiSoftware.Folder.FolderUI
{
	public class DirNameWithID
	{
		public int Id
		{
			get;
			set;
		}

		public int? Pid
		{
			get;
			set;
		}

		// 深度
		public int Level
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public DirNameWithID(int id, int? pid, string name, int lv = 0)
		{
			Id = id;
			Pid = pid;
			Level = lv;
			Name = name;
		}

		public override string ToString()
		{
			string pre = "";

			// 构造前缀
			for (int i = 0; i < Level; i++)
			{
				pre += "/ ";
			}

			return pre + Name;
		}
	}
}