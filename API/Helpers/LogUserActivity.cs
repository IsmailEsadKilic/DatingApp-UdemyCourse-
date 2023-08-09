using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next(); //wait for action to be completed

            if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return; //if user is not authenticated, do nothing

            var UserId = resultContext.HttpContext.User.GetUserId(); //get userName from token

            var uow = resultContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>(); //get user uow

            var user = await uow.UserRepository.GetUserByIdAsync(UserId); //get user from uow
            user.LastActive = DateTime.UtcNow; //set last active to now
            await uow.Complete(); //save changes
        }
    }
}