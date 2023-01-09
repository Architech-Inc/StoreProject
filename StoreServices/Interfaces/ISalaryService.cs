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
		ResponseModel AddSalary(Salary salary);
		ResponseModel AddSalaries(ObservableCollection<Salary> salaries);
		ResponseModel UpdateSalary(Salary salary);
		ResponseModel DeleteSalary(int salaryId);
		ResponseModel DeleteSalaries(ObservableCollection<Salary> salaries);
		ObservableCollection<Salary> GetAllSalaries();
		Salary GetSalary(int salaryId);
	}
}
