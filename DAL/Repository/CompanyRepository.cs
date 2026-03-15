using DAL.DTO;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repository
{
    public class CompanyRepository(Swd392GameAiContext context) : ICompanyRepository
    {
        private readonly Swd392GameAiContext _context = context;

        public List<Company> GetAll(QueryParameters parameters, out int totalCount)
        {
            var query = _context.Companies.AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                var search = parameters.SearchTerm.ToLower();
                query = query.Where(c => 
                    (c.Companyname != null && c.Companyname.ToLower().Contains(search)) || 
                    (c.Country != null && c.Country.ToLower().Contains(search)));
            }

            // Filtering
            if (!string.IsNullOrEmpty(parameters.Filter))
            {
                var filter = parameters.Filter.ToLower();
                query = query.Where(c => c.Country != null && c.Country.ToLower() == filter);
            }

            totalCount = query.Count();

            // Sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                switch (parameters.SortBy.ToLower())
                {
                    case "companyname":
                        query = parameters.IsDescending ? query.OrderByDescending(c => c.Companyname) : query.OrderBy(c => c.Companyname);
                        break;
                    case "country":
                        query = parameters.IsDescending ? query.OrderByDescending(c => c.Country) : query.OrderBy(c => c.Country);
                        break;
                    default:
                        query = query.OrderBy(c => c.Companyid);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(c => c.Companyid);
            }

            // Paging
            return query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();
        }

        public Company? GetById(int id) => _context.Companies.FirstOrDefault(c => c.Companyid == id);

        public void Add(Company company)
        {
            _context.Companies.Add(company);
            _context.SaveChanges();
        }

        public void Update(Company company)
        {
            _context.Companies.Update(company);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var company = _context.Companies.Find(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
                _context.SaveChanges();
            }
        }
    }
}
