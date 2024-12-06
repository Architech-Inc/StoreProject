namespace StoreProjectModels.Data
{
	public class SubCountryWithId
	{
		public int CountryId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Code { get; set; } = string.Empty;
		public int DialCode { get; set; }
	}
}
