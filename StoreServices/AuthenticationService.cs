using Microsoft.IdentityModel.Tokens;
using StoreProjectModels.DatabaseModels;
using StoreProjectModels.Models;
using StoreServices.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace StoreServices
{
	public class AuthenticationService: IAuthenticationService
	{
		private readonly IAuthenticationService _authenticationService;
		private readonly store_dbContext DatabaseContext;
		public AuthenticationService(store_dbContext store_DbContext)
		{
			this.DatabaseContext = store_DbContext;
		}
		public ResponseModel GenerateCode(string username)
		{
			User user = DatabaseContext.Users.Where(u => u.Username == username).FirstOrDefault();
			if (user == null) return new(false, "UserNotFound");
			user.Password = Convert.ToString(Guid.NewGuid());
			UserService userService = new();
			userService.UpdateUser(user);
			return new(true, user.Password);
		}
		public ResponseModel Reset(UserCredential userCredential)
		{
			User user = DatabaseContext.Users.Where(u => u.Username == userCredential.UserName).FirstOrDefault();
			if (user == null) return new(false, "UserNotFound");
			if (user.Password != userCredential.Code) return new(false, "IncorrectCode");
			user.Password = Authentication.EncryptPassword(userCredential.Password);
			UserService userService = new();
			userService.UpdateUser(user);
			return new(true, Authentication.EncryptPassword(userCredential.Password));
		}

		public ResponseModel IsTokenValid(string token)
		{
			try
			{
				if (token == null || ("").Equals(token))
				{
					return new(true, "IsEmpty");
				}

				/***
				 * Make string valid for FromBase64String
				 * FromBase64String cannot accept '.' characters and only accepts stringth whose length is a multitude of 4
				 * If the string doesn't have the correct length trailing padding '=' characters should be added.
				 */
				int indexOfFirstPoint = token.IndexOf('.') + 1;
				String toDecode = token.Substring(indexOfFirstPoint, token.LastIndexOf('.') - indexOfFirstPoint);
				while (toDecode.Length % 4 != 0)
				{
					toDecode += '=';
				}

				//Decode the string
				string decodedString = Encoding.ASCII.GetString(Convert.FromBase64String(toDecode));

				//Get the "exp" part of the string
				Regex regex = new Regex("(\"exp\":)([0-9]{1,})");
				Match match = regex.Match(decodedString);
				long timestamp = Convert.ToInt64(match.Groups[2].Value);

				DateTime date = new DateTime(1970, 1, 1).AddSeconds(timestamp);
				DateTime compareTo = DateTime.UtcNow;

				int result = DateTime.Compare(date, compareTo);

				return new(result > 0, Convert.ToString(result));
			}
			catch (Exception ex)
			{
				return new(false, $"IncorrectToken: {ex.Message}");
			}
		}

		public ResponseModel Authenticate(UserCredential userCredential)
		{
			try
			{
				int days = 1;
				User user = DatabaseContext.Users.Where(u => u.Username == userCredential.UserName).FirstOrDefault();
				if (user == null) return new(false, "UserNotFound");
				if (Crypto.VerifyHashedPassword(user.Password, userCredential.Password) != true) return new(false, "Incorrect");
				JwtSecurityTokenHandler tokenHandler = new();
				byte[] tokenKey = Encoding.ASCII.GetBytes(Authentication.AuthenticationKey);
				days = 1;
				SecurityTokenDescriptor tokenDescriptor = new()
				{
					Subject = new ClaimsIdentity(new Claim[]
					{
						new Claim(ClaimTypes.Name, userCredential.UserName)
					}),
					Expires = DateTime.UtcNow.AddSeconds(600),
					SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
				};
				var token = tokenHandler.CreateToken(tokenDescriptor);
				return new(true, $"{{ \"userId\": {user.UserId},\"token\": \"{tokenHandler.WriteToken(token)}\" }}");
			}
			catch (Exception ex)
			{
				return new(false, $"Check database connections: {ex.Message}");
			}
		}
	}
}
