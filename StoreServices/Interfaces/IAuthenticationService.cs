 using StoreProjectModels.DatabaseModels;
using StoreProjectModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreServices.Interfaces
{
	public interface IAuthenticationService
	{
		ResponseModel GenerateCode(string username);
		ResponseModel Reset(UserCredential userCredential);
		ResponseModel IsTokenValid(string token);
		ResponseModel Authenticate(UserCredential userCredential);
	}
}
