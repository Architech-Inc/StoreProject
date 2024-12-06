using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreProjectModels.Data
{
	[RequiresUnreferencedCode("Necessary because of RangeAttribute usage")]
	public class LoginModel
	{
		public LoginModel()
		{
			PhoneNumber = string.Empty;
			Password = string.Empty;
			Code = string.Empty;
			RememberMe = false;
		}
		public LoginModel(string phoneNumber, string password, bool rememberMe = true, string code = "")
		{
			PhoneNumber = phoneNumber;
			Password = password;
			RememberMe = rememberMe;
			Code = code;
		}

		[Required(ErrorMessage = "Please provide your valid phone number"), MinLength(9), MaxLength(9)]
		public string PhoneNumber { get; set; }
		[Required(ErrorMessage = "Please enter your password")]
		public string Password { get; set; }
		public bool RememberMe { get; set; }
		public string Code { get; set; }
	}
}
