using System.ComponentModel.DataAnnotations;

namespace StoreProjectModels.Data
{
	public class FileObjectInfo
	{
		public string Name { get; set; }
		public string Path { get; set; }
		[MaxLength(1)]
		public string Type { get; set; }
		public long Size { get; set; } = 0;

		public static string GetExtension(string fileName)
		{
			return fileName[fileName.LastIndexOf(".")..];
		}
	}
}
