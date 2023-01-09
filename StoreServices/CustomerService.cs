using StoreProjectModels.CRUD;
using StoreProjectModels.DatabaseModels;
using StoreProjectModels.Models;
using StoreServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreServices
{
	public class CustomerService : ICustomerService
	{
		private readonly store_dbContext DbContext;
		public CustomerService(store_dbContext dbContext)
		{
			DbContext = dbContext;
		}

		public ResponseModel AddCustomer(Customer customer)
		{
			customer.CustomerId = Authentication.GenerateGuid();
			return Crud.Create(customer.CustomerId, customer, DbContext);
		}

		public ResponseModel DeleteCustomer(string customerId)
		{
			return Crud.Delete<Customer>(customerId, DbContext);
		}

		public ResponseModel DeleteCustomers(ObservableCollection<Customer> customers)
		{
			return Crud.DeleteRange<Customer>(customers, DbContext);
		}

		public ObservableCollection<Customer> GetAllCustomers()
		{
			return new(DbContext.Customers);
		}

		public Customer GetCustomer(string customerId)
		{
			return Crud.Read<Customer>(customerId, DbContext);
		}

		public ResponseModel UpdateCustomer(Customer customer)
		{
			return Crud.Update<Customer>(customer.CustomerId, customer, DbContext);
		}
	}
}
