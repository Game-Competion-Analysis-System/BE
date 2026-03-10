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

        public List<Company> GetAll() => _repo.GetAll();
        public Company? GetById(int id) => _repo.GetById(id);
        public void Add(Company company) => _repo.Add(company);
        public void Update(Company company) => _repo.Update(company);
        public void Delete(int id) => _repo.Delete(id);
    }
}
