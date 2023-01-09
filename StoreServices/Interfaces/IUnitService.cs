﻿using StoreProjectModels.DatabaseModels;
using StoreProjectModels.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreServices.Interfaces
{
	public interface IUnitService
	{
		ResponseModel AddUnit(Unit unit);
		ResponseModel UpdateUnit(Unit unit);
		ResponseModel DeleteUnit(int unitId);
		ObservableCollection<Unit> GetAllUnits();
		Unit GetUnit(int unitId);
	}
}
