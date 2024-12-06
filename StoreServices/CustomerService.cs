using StoreProjectModels.CRUD;
using StoreProjectModels.Data;
using StoreProjectModels.DatabaseModels;
using StoreProjectModels.DbContexts;
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
		private readonly StoreDbContext DbContext;
		private readonly ICrud Crud;
		public CustomerService(StoreDbContext dbContext, ICrud crud)
		{
			DbContext = dbContext;
			Crud = crud;
		}

		public CrudResponse AddCustomer(Customer customer)
		{
			customer.CustomerId = Authentication.GenerateGuid();
			return Crud.CreateEntity(customer.CustomerId, customer);
		}

		public CrudResponse DeleteCustomer(string customerId)
		{
			return Crud.DeleteEntity<Customer>(customerId);
		}

		public CrudResponse DeleteCustomers(ObservableCollection<Customer> customers)
		{
			return Crud.DeleteEntities<Customer>(customers);
		}

		public ObservableCollection<Customer> GetAllCustomers()
		{
			return new(DbContext.Customers);
		}

		public Customer GetCustomer(string customerId)
		{
			return Crud.GetEntity<Customer>(customerId);
		}

		public CrudResponse UpdateCustomer(Customer customer)
		{
			return Crud.UpdateEntity<Customer>(customer.CustomerId, customer);
		}
	}
}
