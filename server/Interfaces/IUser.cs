﻿using JinEventsWebAPI.Models;

namespace JinEventsWebAPI.Interfaces
{
	public interface IUser
	{
		public List<User> GetUsers();
		public User GetById(int id);
	}
}
