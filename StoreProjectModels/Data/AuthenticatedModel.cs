namespace StoreProjectModels.Data
{
	public class AuthenticatedModel
	{
		public AuthenticatedModel(string userId, string token)
		{
			UserId = userId;
			Token = token;
		}
		public string UserId { get; set; }
		public string Token { get; set; }
	}
}
