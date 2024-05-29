﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace Luxa.Models

{
	public class Photo
	{
		//[Key]
		public int Id { get; set; }
		[JsonIgnore]
		public UserModel Owner { get; set; } = default!;
		public string OwnerId { get; set; } = default!;
		[DisplayName("Podaj nazwę zdjęcia")]
		public string Name { get; set; } = default!;
		[DisplayName("Podaj opis zdjęcia")]
		public string Description { get; set; } = default!;
		public DateTime AddTime { get; set; } = DateTime.Now;
		//public int LikeCount
		//{
		//	get
		//	{
		//		return UserLikedPhotos.Count(ul => ul.PhotoId == this.Id);
		//	}
		//	private set { }
		//}

		[NotMapped]
		[DisplayName("Zaladuj zdjecie")]
		public IFormFile ImageFile { get; set; } = default!;
		public /*virtual*/ ICollection<UserPhotoModel> UserLikedPhotos { get; set; } = [];
		public ICollection<PhotoTagModel> PhotoTags { get; set; } = [];


	}
}
