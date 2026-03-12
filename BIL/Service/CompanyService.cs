using DAL.DTO;
using DAL.Entities;
using DAL.Repository;
using System.Collections.Generic;

namespace BIL.Service
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _repo;

        public CompanyService(ICompanyRepository repo)
        {
            _repo = repo;
        }

        public List<CompanyDto> GetAll() => _repo.GetAll().Select(c => new CompanyDto
        {
            CompanyId = c.Companyid,
            CompanyName = c.Companyname,
            Country = c.Country,
            Website = c.Website
        }).ToList();
        public Company? GetById(int id) => _repo.GetById(id);
        public void Add(Company company) => _repo.Add(company);
        public void Update(Company company) => _repo.Update(company);
        public void Delete(int id) => _repo.Delete(id);
    }
}
