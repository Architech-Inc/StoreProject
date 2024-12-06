//using Models.Helpers;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace StoreProjectModels.Data
//{
//	public class DeviceInfo
//	{
//		public DeviceInfo()
//		{
//			DeviceId = AuthenticationHelper.GenerateGuidString();
//			DateCreated = DateTime.UtcNow;
//			VisitRecords = new();
//		}
//		public string DeviceId { get; set; } = string.Empty;
//		public string Name { get; set; } = string.Empty;
//		public string IpAddress { get; set; } = string.Empty;
//		public string UserAgent { get; set;} = string.Empty;
//		public bool IsDesktop { get; set; } = false;
//		public string UserToken { get; set; } = string.Empty;
//		public string DeviceHash {  get; set; } = string.Empty;
//		public ScreenInfo Screen { get; set; }
//		public DateTime DateCreated { get; set; }
//		public ObservableCollection<VisitRecord> VisitRecords { get; set; }
//	}

//	public class ScreenInfo
//	{
//		public int Width { get; set; }
//		public int Height { get; set; }
//		public int InnerWidth { get; set; }
//		public int InnerHeight { get; set; }
//	}

//	public class VisitRecord
//	{
//		public VisitRecord()
//		{
//			Id = AuthenticationHelper.GenerateOTP();
//			Date = DateTime.UtcNow;
//			VisitCount = 1;
//		}
//		public int Id { get; set; }
//		public DateTime Date { get; set; }
//		public int VisitCount { get; set; }
//	}
//}
