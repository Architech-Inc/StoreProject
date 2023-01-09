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
		public ResponseModel AddSupplier(Supplier supplier)
		{
			throw new NotImplementedException();
		}

		public ResponseModel DeleteSupplier(string supplierId)
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

		public ResponseModel UpdateSupplier(Supplier supplier)
		{
			throw new NotImplementedException();
		}
	}
}
