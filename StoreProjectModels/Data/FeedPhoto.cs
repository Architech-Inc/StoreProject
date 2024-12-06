using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreProjectModels.Data
{
	// [PrimaryKey("FeedPhotoId")]
	public class FeedPhoto
	{
		public string FeedPhotoId { get; set; }
		public string FeedbackId { get; set; }
		public string PhotoPath { get; set; }
	}
}
