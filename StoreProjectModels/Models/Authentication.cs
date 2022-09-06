using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace StoreProjectModels.Models
{
	public class Authentication
	{
		public static string GenerateGuid()
		{
			return Convert.ToString(Guid.NewGuid());
		}

		public static string AuthenticationKey = "SkillIt Authencation";

		public static string EncryptPassword(string password)
		{
			return Crypto.HashPassword(password);
		}
	}
}
