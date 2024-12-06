using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreProjectModels.Data
{
	//[PrimaryKey("FeedbackId")]
	public class Feedback
	{
		public Feedback() { }

		//[Required]
		public string FeedbackId { get; set; } = string.Empty;

		//[Required]
		public string UserId { get; set; } = string.Empty;

		//[Required]
		//[MinLength(15, ErrorMessage = "Title is too short!")]
		//[StringLength(100, ErrorMessage = "Title too long (100 character limit).")]
		public string Title { get; set; } = string.Empty;

		//[Required]
		//[MinLength(10, ErrorMessage = "Url is too short!")]
		//[StringLength(1000, ErrorMessage = "Url too long (1000 character limit).")]
		public string Url { get; set; } = string.Empty;

		//[Required]
		//[MinLength(100, ErrorMessage = "Description is too short!")]
		public string Description { get; set; } = string.Empty;

		public DateTime DateCreated { get; set; } = DateTime.UtcNow;
		public ObservableCollection<FeedPhoto> FeedPhotos { get; set; } = new();
	}
}
