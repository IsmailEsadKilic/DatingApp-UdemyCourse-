using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace API.Controllers

//hi
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await _userRepository.GetMembersAsync();
            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _userRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return NotFound();

            _mapper.Map(memberUpdateDto, user); //map memberUpdateDto to user

            if (await _userRepository.SaveAllAsync()) return NoContent(); //if save is successful, return no content

            return BadRequest("Failed to update user"); //if save is unsuccessful, return bad request
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername()); //get user from repo

            if (user == null) return NotFound(); //if user is null, return not found

            var result = await _photoService.AddPhotoAsync(file); //add photo to cloudinary

            if (result.Error != null) return BadRequest(result.Error.Message); //if there is an error, return bad request

            var photo = new Photo //create new photo object
            {
                Url = result.SecureUrl.AbsoluteUri, //set url to the secure url
                PublicId = result.PublicId //set public id to the public id
            };

            if (user.Photos.Count == 0) //if user has no photos
            {
                photo.IsMain = true; //set photo to main
            }


            user.Photos.Add(photo); //add photo to user

            if (await _userRepository.SaveAllAsync()) //if save is successful
            {
                return CreatedAtAction(nameof(GetUser), new { username = user.Username }, _mapper.Map<PhotoDto>(photo)); //return created at action
            }

            return BadRequest("Problem adding photo"); //if save is unsuccessful, return bad request

        }

         [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername()); //get user from repo using username from token
            
            if (user == null) return NotFound(); //if user is null, return not found

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId); //get photo from user

            if (photo == null) return NotFound(); //if photo is null, return not found

            if (photo.IsMain) return BadRequest("This is already your main photo"); //if photo is already main, return bad request

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain); //get current main photo

            if (currentMain != null) currentMain.IsMain = false; //if current main is not null, set to false

            photo.IsMain = true; //set photo to main

            if (await _userRepository.SaveAllAsync()) return NoContent(); //if save is successful, return no content

            return BadRequest("Failed to set main photo"); //if save is unsuccessful, return bad request
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return NotFound(); //if user is null, return not found

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId); //get photo from user

            if (photo == null) return NotFound(); //if photo is null, return not found

            if (photo.IsMain) return BadRequest("You cannot delete your main photo"); //if photo is main, return bad request

            if (photo.PublicId != null) //if public id is not null
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId); //delete photo from cloudinary

                if (result.Error != null) return BadRequest(result.Error.Message); //if there is an error, return bad request
            }

            user.Photos.Remove(photo); //remove photo from user

            if (await _userRepository.SaveAllAsync()) return Ok(); //if save is successful, return ok

            return BadRequest("Failed to delete photo"); //if save is unsuccessful, return bad request 
        }
    }
}