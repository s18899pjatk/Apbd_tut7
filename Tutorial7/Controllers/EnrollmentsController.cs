using apbd_tut7.DTO.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tutorial7.Services;

namespace apbd_tut7.Controllers
{

    [Route("api/enrollments")]
    [Authorize(Roles = "Employee")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private IStudentsSerivceDb _service;

        public EnrollmentsController(IStudentsSerivceDb serivice)
        {
            _service = serivice;
        }

        [HttpPost(Name = nameof(EnrollStudent))]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            var response = _service.EnrollStudent(request);
            if (response == null) return BadRequest("Cannot create such a student");
            else return CreatedAtAction(nameof(EnrollStudent), response);
        }

        [HttpPost("promotions", Name = nameof(PromoteStudents))]
        public IActionResult PromoteStudents(PromoteStudentRequest request)
        {
            var response = _service.PromoteStudent(request);
            return CreatedAtAction(nameof(PromoteStudents), response);
        }
    }
}