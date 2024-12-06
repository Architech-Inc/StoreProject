using StoreProjectModels.Data;
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
		CrudResponse GenerateCode(string username);
		CrudResponse Reset(UserCredential userCredential);
		CrudResponse IsTokenValid(string token);
		CrudResponse Authenticate(UserCredential userCredential);
	}
}
