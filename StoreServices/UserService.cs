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
    public class UserService : IUserService
    {
        private readonly store_dbContext DbContext;
        public ResponseModel AddUser(User user)
        {
            if (user == null) return new(false, "UserNull");
            try
            {
                user.UserId = Authentication.GenerateGuid();
                if (DbContext.Users.Where(u => u.UserId == user.UserId).ToList().FirstOrDefault() != null) return new(false, "UserExists");
                DbContext.Users.Add(user);
                DbContext.SaveChanges();
                return new(true, "Success");
            }
            catch (Exception ex)
            {
                return new(false, $"Failed: {ex}");
            }
        }

        public ResponseModel DeleteUser(string userId)
        {
            try
            {
                User user = DbContext.Users.Where(u => u.UserId == userId).FirstOrDefault();
				if (user == null) return new(false, "UserNotFound");
				DbContext.Entry<User>(user).State = EntityState.Deleted;
                DbContext.Users.Remove(user);
                DbContext.SaveChanges();
				return new(true, "Success");
            }
            catch (Exception ex)
            {
                return new(false, $"Failed: {ex}");
            }
        }

        public ObservableCollection<User> GetAllUsers()
        {
            return new ObservableCollection<User>(DbContext.Users);
        }

        public User GetUser(string userId)
        {
            User user = DbContext.Users.Where(u => u.UserId == userId).FirstOrDefault();
            if (user == null) return null;
            return user;
        }

        public ResponseModel UpdateUser(User user)
        {
            User _user = DbContext.Users.Where(u => u.UserId == user.UserId).ToList().FirstOrDefault();
			if (user == null) return new(false, "UserNotFound");
			try
			{
                _user = user;
                DbContext.Entry<User>(_user).State = EntityState.Detached;
				DbContext.Users.Update(_user);
				DbContext.SaveChanges();
				return new(true, "Success");
			}
			catch (Exception ex)
			{
				return new(false, $"Failed: {ex}");
			}
		}
    }
}
