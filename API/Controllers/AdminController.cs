using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IPhotoService _photoService;
        public AdminController(UserManager<AppUser> userManager, IPhotoService photoService)
        {
            _photoService = photoService;
            _userManager = userManager;
        }
        
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .Select(u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")] //username is the route parameter
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            if (string.IsNullOrEmpty(roles)) return BadRequest("No roles were selected"); //if roles is null or empty, return bad request

            var selectedRoles = roles.Split(",").ToArray(); //split the roles string into an array

            var user = await _userManager.FindByNameAsync(username); //find the user by username

            if (user == null) return NotFound("Could not find user"); //if user is null, return not found

            var userRoles = await _userManager.GetRolesAsync(user); //get the roles for the user

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles)); //add the roles to the user

            if (!result.Succeeded) return BadRequest("Failed to add to roles"); //if the result is not successful, return bad request

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles)); //remove the roles from the user

            if (!result.Succeeded) return BadRequest("Failed to remove the roles"); //if the result is not successful, return bad request

            return Ok(await _userManager.GetRolesAsync(user)); //return the roles for the user
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Admins or moderators can see this");
        }
    }
}