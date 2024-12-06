using StoreProjectModels.Data;
using System.Text;

namespace Models.Helpers
{
	public class HttpHelper
	{
		public HttpHelper()
		{
				
		}

		public static async Task<bool> CheckConnectionAsync(string url, HttpClient client, string token = "")
		{
			if (url.Contains("ngrok")) client.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "69420");
			if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new("Bearer", token);
			var result = await client.GetAsync(url);
			if(result.StatusCode == System.Net.HttpStatusCode.OK)
			{
				return true;
			}
			return false;
		}

		public static async Task<RequestResponse> PostFormFileAsync(string url, MultipartFormDataContent formDataContent, HttpClient client, string token = "")
		{
			if(url.Contains("ngrok")) client.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "69420");
			if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new("Bearer", token);
			try
			{
				var response = await client.PostAsync(url, formDataContent);
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					//string jsonStr = await response.Content.ReadAsStringAsync();
					//return JsonConvert.DeserializeObject<ObservableCollection<SubCountry>>(jsonStr);
					var res = new RequestResponse(true, "Success", response.ReasonPhrase!)
					{
						Data = await response.Content.ReadAsStringAsync()
					};
					return res;
				}
				else
				{
					var res = new RequestResponse(false, $"There is an issues with the server. Please try again and if this persist then report the error.");
					//var res = new RequestResponse(false, $"Failed: {response.StatusCode} \n\n{await response.Content.ReadAsStringAsync()}");
					return res;
				}
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("System.Net.WebException: 'SSL handshake aborted:") || ex.Message.Contains("System.Net.Http.HttpRequestException: 'The SSL connection could not be established"))
				{
					return new(false, "It seems you have no data bundle active.");
				}
				else if (ex.Message.Contains("System.Net.WebException: 'Unable to resolve host") || ex.Message.Contains("System.Net.Http.HttpRequestException: 'No such host is known"))
				{
					return new(false, "You have no Internet connection");
				}
				else
				{
					//return new(false, ex.Message);
					return new(false, "Unable to connect to server");
				}
			}
		}

		public static async Task<RequestResponse> PostAsync(string url, string content, HttpClient client, string token = "")
		{
			if (url.Contains("ngrok")) client.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "69420");
			if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new("Bearer", token);
			try
			{
				var response = await client.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json"));
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					//string jsonStr = await response.Content.ReadAsStringAsync();
					//return JsonConvert.DeserializeObject<ObservableCollection<SubCountry>>(jsonStr);
					var res = new RequestResponse(true, "Success")
					{
						Data = await response.Content.ReadAsStringAsync()
					};
					return res;
				}
				else
				{
					var res = new RequestResponse(false, $"There is an issues with the server. Please try again and if this persist then report the error.");
					//var res = new RequestResponse(false, $"Failed: {response.StatusCode} \n\n{await response.Content.ReadAsStringAsync()}");
					return res;
				}
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("System.Net.WebException: 'SSL handshake aborted:") || ex.Message.Contains("System.Net.Http.HttpRequestException: 'The SSL connection could not be established"))
				{
					return new(false, "It seems you have no data bundle active.");
				}
				else if (ex.Message.Contains("System.Net.WebException: 'Unable to resolve host") || ex.Message.Contains("System.Net.Http.HttpRequestException: 'No such host is known"))
				{
					return new(false, "You have no Internet connection");
				}
				else
				{
					return new(false, "Unable to connect to server");
				}
			}
		}

		public static async Task<RequestResponse> GetAsync(string url, HttpClient client, string token = "")
		{
			if (url.Contains("ngrok")) client.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "69420");
			if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new("Bearer", token);
			client.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "69420");
			try
			{
				var response = await client.GetAsync(url);
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					//string jsonStr = await response.Content.ReadAsStringAsync();
					//return JsonConvert.DeserializeObject<ObservableCollection<SubCountry>>(jsonStr);
					var res = new RequestResponse(true, "Success")
					{
						Data = await response.Content.ReadAsStringAsync()
					};
					return res;
				}
				else
				{
					var res = new RequestResponse(false, $"There is an issues with the server. Please try again and if this persist then report the error.");
					//var res = new RequestResponse(false, $"Failed: {response.StatusCode} \n\n{await response.Content.ReadAsStringAsync()}");
					return res;
				}
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("System.Net.WebException: 'SSL handshake aborted:") || ex.Message.Contains("System.Net.Http.HttpRequestException: 'The SSL connection could not be established"))
				{
					return new(false, "It seems you have no data bundle active.");
				}
				else if (ex.Message.Contains("System.Net.WebException: 'Unable to resolve host") || ex.Message.Contains("System.Net.Http.HttpRequestException: 'No such host is known"))
				{
					return new(false, "You have no Internet connection");
				}
				else
				{
					return new(false, "Unable to connect to server");
				}
			}
		}
	}
}
