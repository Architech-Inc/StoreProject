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
	public interface ISupplierService
	{
		ResponseModel AddSupplier(Supplier supplier);
		ResponseModel UpdateSupplier(Supplier supplier);
		ResponseModel DeleteSupplier(string supplierId);
		ObservableCollection<Supplier> GetAllSuppliers();
		Supplier GetSupplier(string supplierId);
	}
}
