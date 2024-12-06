using Microsoft.EntityFrameworkCore;
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
	public class UnitService : IUnitService
	{
		private readonly StoreDbContext DbContext;
        private readonly ICrud Crud;
        public UnitService(StoreDbContext dbContext, ICrud crud)
        {
            DbContext = dbContext;
            Crud = crud;
        }

        public CrudResponse AddUnit(Unit unit)
		{
			if (unit == null) return new(false, "UnitNull");
			try
			{
				DbContext.Units.Add(unit);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed due to: {ex.Message}");
			}
		}

		public CrudResponse DeleteUnit(int unitId)
		{
			Unit unit = DbContext.Units.Find(unitId);
			if (unit == null) return new(false, "UnitNotFound");
			try
			{
				DbContext.Entry(unit).State = EntityState.Detached;
				DbContext.Remove(unit);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed due to: {ex.Message}");
			}
		}

		public ObservableCollection<Unit> GetAllUnits()
		{
			return new ObservableCollection<Unit>(DbContext.Units);
		}

		public Unit GetUnit(int unitId)
		{
			return DbContext.Units.Find(unitId);
		}

		public CrudResponse UpdateUnit(Unit unit)
		{
			if (unit == null) return new(false, "UnitNull");
			try
			{
				DbContext.Entry(unit).State = EntityState.Detached;
				DbContext.Update(unit);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed due to: {ex.Message}");
			}
		}
	}
}
