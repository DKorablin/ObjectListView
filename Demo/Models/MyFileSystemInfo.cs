using System;
using System.Collections;
using System.IO;

namespace ObjectListViewDemo.Models
{
	/// <summary>
	/// Standard .NET FileSystemInfos are always not equal to each other.
	/// When we try to refresh a directory, our controls can't match up new
	/// files with existing files. They are also sealed so we can't just subclass them.
	/// This class is a wrapper around a FileSystemInfo that simply provides
	/// equality.
	/// </summary>
	public class MyFileSystemInfo : IEquatable<MyFileSystemInfo>
	{
		public MyFileSystemInfo(FileSystemInfo fileSystemInfo)
			=> this.Info = fileSystemInfo ?? throw new ArgumentNullException(nameof(fileSystemInfo));

		public Boolean IsDirectory { get => this.AsDirectory != null; }

		public DirectoryInfo AsDirectory { get => this.Info as DirectoryInfo; }
		public FileInfo AsFile { get => this.Info as FileInfo; }

		public FileSystemInfo Info { get; }

		public String Name { get => this.Info.Name; }

		public String Extension { get => this.Info.Extension; }

		public DateTime CreationTime { get => this.Info.CreationTime; }

		public DateTime LastWriteTime { get => this.Info.LastWriteTime; }

		public String FullName { get => this.Info.FullName; }

		public FileAttributes Attributes { get => this.Info.Attributes; }

		public Int64 Length { get => this.AsFile.Length; }

		public IEnumerable GetFileSystemInfos()
		{
			ArrayList children = new ArrayList();
			if(this.IsDirectory)
				foreach(FileSystemInfo x in this.AsDirectory.GetFileSystemInfos())
					children.Add(new MyFileSystemInfo(x));
			return children;
		}

		// Two file system objects are equal if they point to the same file system path

		public Boolean Equals(MyFileSystemInfo other)
		{
			if(ReferenceEquals(null, other)) return false;
			if(ReferenceEquals(this, other)) return true;
			return Equals(other.Info.FullName, this.Info.FullName);
		}
		public override Boolean Equals(Object obj)
		{
			if(ReferenceEquals(null, obj)) return false;
			if(ReferenceEquals(this, obj)) return true;
			if(obj.GetType() != typeof(MyFileSystemInfo)) return false;
			return Equals((MyFileSystemInfo)obj);
		}
		public override Int32 GetHashCode()
			=> (this.Info != null ? this.Info.FullName.GetHashCode() : 0);

		public static Boolean operator ==(MyFileSystemInfo left, MyFileSystemInfo right)
			=> Equals(left, right);

		public static Boolean operator !=(MyFileSystemInfo left, MyFileSystemInfo right)
			=> !Equals(left, right);
	}
}