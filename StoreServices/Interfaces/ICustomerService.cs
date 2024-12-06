using StoreProjectModels.Data;
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
		CrudResponse AddCustomer(Customer customer);
		CrudResponse UpdateCustomer(Customer customer);
		CrudResponse DeleteCustomer(string customerId);
		CrudResponse DeleteCustomers(ObservableCollection<Customer> customers);
		ObservableCollection<Customer> GetAllCustomers();
		Customer GetCustomer(string customerId);
	}
}
