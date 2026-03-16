using DAL.DTO;
using DAL.Entities;
using DAL.Repository;
using System.Collections.Generic;

namespace BIL.Service
{
    public class EventService(IEventRepository repo) : IEventService
    {
        public PagedResult<EventDto> GetAll(QueryParameters parameters)
        {
            var events = repo.GetAll(parameters, out int totalCount);
            return new PagedResult<EventDto>
            {
                Items = events.Select(e => new EventDto
                {
                    EventId = e.Eventid,
                    EventName = e.Eventname,
                    EventType = e.Eventtype,
                    StartDate = e.Startdate,
                    EndDate = e.Enddate,
                    GameName = e.Game?.Gamename
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }
        public EventDto? GetById(int id)
        {
            var e = repo.GetById(id);
            return e == null ? null : new EventDto
            {
                EventId = e.Eventid,
                EventName = e.Eventname,
                EventType = e.Eventtype,
                StartDate = e.Startdate,
                EndDate = e.Enddate,
                GameName = e.Game?.Gamename
            };
        }

        public List<EventDto> GetByGame(int gameId) => repo.GetByGame(gameId).Select(e => new EventDto
        {
            EventId = e.Eventid,
            EventName = e.Eventname,
            EventType = e.Eventtype,
            StartDate = e.Startdate,
            EndDate = e.Enddate,
            GameName = e.Game?.Gamename
        }).ToList();

        public List<EventDto> SearchByName(string name) => repo.SearchByName(name).Select(e => new EventDto
        {
            EventId = e.Eventid,
            EventName = e.Eventname,
            EventType = e.Eventtype,
            StartDate = e.Startdate,
            EndDate = e.Enddate,
            GameName = e.Game?.Gamename
        }).ToList();
        public void Add(Event @event) => repo.Add(@event);
        public void Update(Event @event) => repo.Update(@event);
        public void Delete(int id) => repo.Delete(id);
    }
}
