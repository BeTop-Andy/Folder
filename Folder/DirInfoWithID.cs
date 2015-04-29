using System.IO;
using System.Runtime.Serialization;

namespace HuaweiSoftware.Folder
{
	[DataContractAttribute]
	public class DirInfoWithID
	{
		[DataMember]
		public int Id
		{
			get;
			set;
		}

		[DataMember]
		public int? Pid
		{
			get;
			set;
		}

		[DataMember]
		public DirectoryInfo Info
		{
			get;
			set;
		}

		public DirInfoWithID(int id, int? pid, DirectoryInfo di)
		{
			Id = id;
			Pid = pid;
			Info = di;
		}

		public override string ToString()
		{
			return Info.FullName;
		}
	}
}