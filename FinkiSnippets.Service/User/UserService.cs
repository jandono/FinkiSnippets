﻿using FinkiSnippets.Data;
using FinkiSnippets.Service.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity;
using System.Data.Entity;

namespace FinkiSnippets.Service
{
    public class UserService : IUserService
    {
        private CodeDatabase db;

        public UserService(CodeDatabase _db)
        {
            db = _db;
        }

        public EventSnippets BeginEvent(string UserID, int EventID)
        {
            var checkIfAlreadyFinished = db.UserEvents.Any(x => x.UserID == UserID && x.EventID == EventID && x.Finished);
            if (checkIfAlreadyFinished)
                return null;

            UserEvents userEvent = new UserEvents
            {
                UserID = UserID,
                EventID = EventID,
                Finished = false
            };
            db.UserEvents.Add(userEvent);
            int res = db.SaveChanges();

            var firstSnippet = db.EventSnippets.Include(x => x.Snippet).FirstOrDefault(x => x.EventID == EventID);
            return firstSnippet;
        }

        public ListUsersDto GetAllUsers(int page, int usersPerPage)
        {
            var query = db.Users.OrderBy(x => x.UserName);

            var tempResult = query.Skip((page - 1) * usersPerPage).Take(usersPerPage).Select(x => new
            {
                x.Id,
                x.FirstName,
                x.LastName,
                x.UserName
            });

            ListUsersDto result = new ListUsersDto
            {
                TotalCount = query.Count(),
                Users = tempResult.Select(x => new UserDto
                {
                    ID = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Username = x.UserName
                }).ToList()
            };

            return result;
        }

        public EventSnippets UserActiveEvent(string UserID)
        {
            UserEvents activeEvent = db.UserEvents.Include(x => x.Event).Where(x => x.UserID == UserID && !x.Finished).FirstOrDefault();

            if (activeEvent == null)
                return null;

            if (activeEvent.Event.End < Utilities.DateHelper.GetCurrentTime())
            {
                activeEvent.Finished = true;
                db.SaveChanges();
                return null;
            }

            EventSnippets result = db.UserEvents.Include(x => x.Event).Include(x => x.Event.EventSnippets).Where(x => x.UserID == UserID && !x.Finished)
                .SelectMany(x => x.Event.EventSnippets.Where(y => y.OrderNumber == x.Event.EventSnippets.Max(t => t.OrderNumber))).FirstOrDefault();

            //if()

            //EventSnippets result = activeEvent.Event.EventSnippets.SelectMany(x => x.Event.EventSnippets.Where(y => y.OrderNumber == x.Event.EventSnippets.Max(t => t.OrderNumber))).FirstOrDefault();

            return result;
        }
    }
}
