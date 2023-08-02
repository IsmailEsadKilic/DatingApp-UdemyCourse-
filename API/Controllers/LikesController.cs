using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;
        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            _likesRepository = likesRepository;
            _userRepository = userRepository;
        }

        [HttpPost("{username}")] //post to api/likes/username
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId(); //get user id
            var likedUser = await _userRepository.GetUserByUsernameAsync(username); //get user by username
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId); //get user with likes

            if (likedUser == null) return NotFound(); //if user is not found, return not found

            if (sourceUser.Username == username) return BadRequest("You cannot like yourself"); //if user tries to like themselves, return bad request

            var userLike = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id); //get user like

            if (userLike != null) return BadRequest("You already like this user"); //if user already likes this user, return bad request

            userLike = new UserLike //create new user like
            {
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike); //add user like to source user

            if (await _userRepository.SaveAllAsync()) return Ok(); //if save is successful, return ok

            return BadRequest("Failed to like user"); //if save is unsuccessful, return bad request

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes(string predicate)
        {
            var users = await _likesRepository.GetUserLikes(predicate, User.GetUserId()); //get user likes
            return Ok(users); //return users
        }
    }
}