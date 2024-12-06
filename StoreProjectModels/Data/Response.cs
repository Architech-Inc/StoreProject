using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreProjectModels.Data
{
	public class Response
	{
		public Response()
		{
				
		}
		public Response(bool success, string message)
		{
			Success = success;
			Message = message;
		}
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
	}
}
