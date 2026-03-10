using DAL.Entities;
using System.Collections.Generic;

namespace BIL.Service
{
    public interface ICompanyService
    {
        List<Company> GetAll();
        Company? GetById(int id);
        void Add(Company company);
        void Update(Company company);
        void Delete(int id);
    }
}
