using Microsoft.EntityFrameworkCore;
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
	public class UnitService : IUnitService
	{
		private readonly store_dbContext DbContext;
		public UnitService(store_dbContext dbContext)
		{
			DbContext = dbContext;
		}

		public ResponseModel AddUnit(Unit unit)
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

		public ResponseModel DeleteUnit(int unitId)
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

		public ResponseModel UpdateUnit(Unit unit)
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
