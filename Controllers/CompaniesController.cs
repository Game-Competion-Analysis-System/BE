using BIL.Service;
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
        private readonly ICompanyService _service = service;

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll() => Ok(_service.GetAll());

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult GetById(int id)
        {
            var company = _service.GetById(id);
            if (company == null) return NotFound();
            return Ok(company);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Create(Company company)
        {
            _service.Add(company);
            return Ok(company);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Update(int id, [FromBody] Company company)
        {
            company.Companyid = id;
            _service.Update(company);
            return Ok(company);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return Ok(new { message = "Company deleted successfully" });
        }
    }
}
