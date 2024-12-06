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
	public class SalaryService : ISalaryService
	{
		private readonly StoreDbContext DbContext;
        private readonly ICrud Crud;
        public SalaryService(StoreDbContext dbContext, ICrud crud)
        {
            DbContext = dbContext;
            Crud = crud;
        }

        public CrudResponse AddSalary(Salary salary)
		{
			return Crud.CreateEntity<Salary>(salary.SalaryId, salary);
		}

		public CrudResponse AddSalaries(ObservableCollection<Salary> salaries)
		{
			return Crud.CreateEntities<Salary>(nameof(Salary.SalaryId), salaries);
		}

		public CrudResponse DeleteSalaries(ObservableCollection<Salary> salaries)
		{
			return Crud.DeleteEntities<Salary>(salaries);
		}

		public CrudResponse DeleteSalary(int salaryId)
		{
			return Crud.DeleteEntity<Salary>(salaryId);
		}

		public ObservableCollection<Salary> GetAllSalaries()
		{
			return new(DbContext.Salaries);
		}

		public Salary GetSalary(int salaryId)
		{
			return DbContext.Salaries.Find(salaryId);
		}

		public CrudResponse UpdateSalary(Salary salary)
		{
			return Crud.UpdateEntity<Salary>(salary.SalaryId, salary);
		}
	}
}
