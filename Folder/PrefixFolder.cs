using System.IO;

namespace HuaweiSoftware.Folder
{
	public class PrefixFolder
	{
		public DirectoryInfo DirInfo
		{
			get;
			set;
		}
		public string Prefix
		{
			get;
			set;
		}

		public PrefixFolder(string pre, DirectoryInfo di)
		{
			DirInfo = di;
			Prefix = pre;
		}

		public override string ToString()
		{
			return Prefix == "." ? "." : Prefix + DirInfo.Name;
		}
	}
}
