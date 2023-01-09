using StoreProjectModels.DatabaseModels;
using StoreProjectModels.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreServices.Interfaces
{
	public interface ICustomerService
	{
		ResponseModel AddCustomer(Customer customer);
		ResponseModel UpdateCustomer(Customer customer);
		ResponseModel DeleteCustomer(string customerId);
		ResponseModel DeleteCustomers(ObservableCollection<Customer> customers);
		ObservableCollection<Customer> GetAllCustomers();
		Customer GetCustomer(string customerId);
	}
}
