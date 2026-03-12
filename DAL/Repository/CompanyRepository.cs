using DAL.DTO;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repository
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly Swd392GameAiContext _context;

        public CompanyRepository(Swd392GameAiContext context)
        {
            _context = context;
        }

        public List<Company> GetAll() => _context.Companies.ToList();

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
