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
	public interface ISalaryService
	{
		CrudResponse AddSalary(Salary salary);
		CrudResponse AddSalaries(ObservableCollection<Salary> salaries);
		CrudResponse UpdateSalary(Salary salary);
		CrudResponse DeleteSalary(int salaryId);
		CrudResponse DeleteSalaries(ObservableCollection<Salary> salaries);
		ObservableCollection<Salary> GetAllSalaries();
		Salary GetSalary(int salaryId);
	}
}
