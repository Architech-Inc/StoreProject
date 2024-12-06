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
	public class NewsLetterFormModel
	{
		[Required(ErrorMessage = "Email is required."),
			RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$", ErrorMessage = "Email must be in such format,'example@gmail.com'")]
		public string Email { get; set; } = string.Empty;
	}
}
