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
	public class SalaryService : ISalaryService
	{
		private readonly store_dbContext DbContext;
		public SalaryService(store_dbContext dbContext)
		{
			DbContext = dbContext;
		}

		public ResponseModel AddSalary(Salary salary)
		{
			return Crud.Create<Salary>(salary.SalaryId, salary, DbContext);
		}

		public ResponseModel AddSalaries(ObservableCollection<Salary> salaries)
		{
			return Crud.CreateRange<Salary>(salaries, DbContext);
		}

		public ResponseModel DeleteSalaries(ObservableCollection<Salary> salaries)
		{
			return Crud.DeleteRange<Salary>(salaries, DbContext);
		}

		public ResponseModel DeleteSalary(int salaryId)
		{
			return Crud.Delete<Salary>(salaryId, DbContext);
		}

		public ObservableCollection<Salary> GetAllSalaries()
		{
			return new(DbContext.Salaries);
		}

		public Salary GetSalary(int salaryId)
		{
			return DbContext.Salaries.Find(salaryId);
		}

		public ResponseModel UpdateSalary(Salary salary)
		{
			return Crud.Update<Salary>(salary.SalaryId, salary, DbContext);
		}
	}
}
