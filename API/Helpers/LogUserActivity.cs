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

            var UserId = resultContext.HttpContext.User.GetUserId(); //get username from token

            var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>(); //get user repo

            var user = await repo.GetUserByIdAsync(UserId); //get user from repo
            user.LastActive = DateTime.UtcNow; //set last active to now
            await repo.SaveAllAsync(); //save changes
        }
    }
}