using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreProjectModels.Data
{
	public class ResetModel
	{
		public string UserId { get; set; } = string.Empty;
		public long OtpId { get; set; }
		public int Otp { get; set; }
		public string Password { get; set; } = string.Empty;
		public string Guid { get; set; } = string.Empty;
	}
}
