using DAL.DTO;
using DAL.Entities;
using DAL.Repository;
using System.Collections.Generic;

namespace BIL.Service
{
    public class CompanyService(ICompanyRepository repo) : ICompanyService
    {
        public PagedResult<CompanyDto> GetAll(QueryParameters parameters)
        {
            var companies = repo.GetAll(parameters, out int totalCount);
            return new PagedResult<CompanyDto>
            {
                Items = companies.Select(c => new CompanyDto
                {
                    CompanyId = c.Companyid,
                    CompanyName = c.Companyname,
                    Country = c.Country,
                    Website = c.Website
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }
        public CompanyDto? GetById(int id)
        {
            var c = repo.GetById(id);
            return c == null ? null : new CompanyDto
            {
                CompanyId = c.Companyid,
                CompanyName = c.Companyname,
                Country = c.Country,
                Website = c.Website
            };
        }
        public void Add(Company company) => repo.Add(company);
        public void Update(Company company) => repo.Update(company);
        public void Delete(int id) => repo.Delete(id);
    }
}
