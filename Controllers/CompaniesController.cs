using BIL.Service;
using DAL.DTO;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GameCompetionAnalysisSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompaniesController(ICompanyService service) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetList([FromQuery] QueryParameters parameters) => Ok(service.GetAll(parameters));

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetById(int id)
        {
            var company = service.GetById(id);
            if (company == null) return NotFound();
            return Ok(company);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Create(Company company)
        {
            service.Add(company);
            return Ok(new CompanyDto
            {
                CompanyId = company.Companyid,
                CompanyName = company.Companyname,
                Country = company.Country,
                Website = company.Website
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(int id, [FromBody] Company company)
        {
            company.Companyid = id;
            service.Update(company);
            return Ok(new CompanyDto
            {
                CompanyId = company.Companyid,
                CompanyName = company.Companyname,
                Country = company.Country,
                Website = company.Website
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            service.Delete(id);
            return Ok(new { message = "Delete successful" });
        }
    }
}
