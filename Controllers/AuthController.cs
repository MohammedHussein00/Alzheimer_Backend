using Alzheimer.Models;
using Alzheimer.Services;
using Alzheimer.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Alzheimer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _Context;

       

        private readonly IAuthService _authService;
        public AuthController(IAuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _Context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Index(string x)
        {
            return Ok(x);
        }
        [HttpPost("Register")]

        public async Task<IActionResult> RegisterAsync(RegisterModel model) {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result =await _authService.RegisterAsync(model);
            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost("Token")]
        
        public async Task<IActionResult> GetTokenAsync(TokenRequestModel model)
        {
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);

            var result = await _authService.GetTokenAsync(model);
            if (!result.IsAuthenticated)
                return Ok(result.Message);

            return Ok(result);
        }

        [HttpPost("AddRole")]

        public async Task<IActionResult> AddRoleAsync(AddRoleModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result =await _authService.AddRoleAsync(model);
            if (!string.IsNullOrEmpty(result))
                return BadRequest(result);


            return Ok(model);
        }
        [Authorize(Roles = "User")]
        [HttpPut("{Id}")]
        public async Task<IActionResult> Update(int id,Patient c)
        {
            var isfound = await _Context.Patients.FirstOrDefaultAsync(i => i.Id == c.Id);
            if (isfound == null)
                return NotFound();
            var x = _authService.EditAsync(c);
            return Ok(x);
        }
        [HttpGet("{Email}")]
        public async Task<IActionResult> GetName(string Email)
        {
            var customer = await _Context.Patients.FirstOrDefaultAsync(i => i.Email == Email);
            if (customer == null)
                return NotFound();

            var response = new { Name = customer.Name };
            return Ok(response);
        }

    }
}
