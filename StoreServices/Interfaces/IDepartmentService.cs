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
	public interface IDepartmentService
	{
		ResponseModel AddDepartment(Department user);
		ResponseModel UpdateDepartment(Department user);
		ResponseModel DeleteDepartment(int deptId);
		ObservableCollection<Department> GetAllDepartments();
		Department GetDepartment(int deptId);
	}
}
