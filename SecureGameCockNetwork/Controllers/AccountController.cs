using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SecureGameCockNetwork.Controllers
{
    public class AccountController : Controller
    {
        //To Do : Update this file accordingly with account related actions.
        // GET: /Account/

        public ActionResult Index()
        {
            return View();
        }

        //ToDo : Registration Process

        // POST api/Account/Register
        //[AllowAnonymous]
        //[Route("Register")]
        //public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    IdentityUser user = new IdentityUser
        //    {
        //        UserName = model.UserName
        //    };

        //    IdentityResult result = await UserManager.CreateAsync(user, model.Password);
        //    IHttpActionResult errorResult = GetErrorResult(result);

        //    if (errorResult != null)
        //    {
        //        return errorResult;
        //    }

        //    return Ok();
        //}
    }
}
