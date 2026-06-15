using DAL.DTO;
using DAL.Entities;
using System.Collections.Generic;

namespace DAL.Repository
{
    public interface ICompanyRepository
    {
        List<Company> GetAll(QueryParameters parameters, out int totalCount);
        Company? GetById(int id);
        void Add(Company company);
        void Update(Company company);
        void Delete(int id);
    }
}
