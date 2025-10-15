using Microsoft.AspNetCore.Mvc;
using SchoolApp.DTO;
using SchoolApp.Exceptions;
using SchoolApp.Services;

namespace SchoolApp.Controllers
{
    public class UsersController : BaseController
    {
        private readonly IConfiguration configuration;

        public UsersController(IApplicationService applicationService, IConfiguration configuration) : 
            base(applicationService)
        {
            this.configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult<UserReadOnlyDTO> SignupUserTeacherAsync(TeacherSignupDTO teacherSignupDTO)
        {
           if (!ModelState.IsValid)
           {
               var errors = ModelState.Where(e => e.Value!.Errors.Any())
                   .Select(e => new { 
                       Field = e.Key, 
                       Errors = e.Value!.Errors.Select(er => er.ErrorMessage).ToArray()
                   });
                throw new InvalidRegistrationException("ErrorsInRegistration" + errors);
           }

           UserReadOnlyDTO returnedUserDTO = await applicationService.TeacherService.SignupUserAsync(teacherSignupDTO);
        }


    }
}
