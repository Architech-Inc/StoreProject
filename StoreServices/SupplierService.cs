using StoreProjectModels.Data;
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
	public class SupplierService : ISupplierService
	{
		public CrudResponse AddSupplier(Supplier supplier)
		{
			throw new NotImplementedException();
		}

		public CrudResponse DeleteSupplier(string supplierId)
		{
			throw new NotImplementedException();
		}

		public ObservableCollection<Supplier> GetAllSuppliers()
		{
			throw new NotImplementedException();
		}

		public Supplier GetSupplier(string supplierId)
		{
			throw new NotImplementedException();
		}

		public CrudResponse UpdateSupplier(Supplier supplier)
		{
			throw new NotImplementedException();
		}
	}
}
