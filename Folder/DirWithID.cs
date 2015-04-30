using System.IO;
using System.Runtime.Serialization;

namespace HuaweiSoftware.Folder
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
			for (int i = 0; i < Level; i++)
			{
				pre += "/ ";
			}

			return pre+Name;
		}
	}
}