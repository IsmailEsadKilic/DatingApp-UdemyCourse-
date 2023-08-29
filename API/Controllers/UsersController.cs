using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using SQLitePCL;

namespace API.Controllers

//hi
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IPhotoService _photoService;
        public UsersController(IUnitOfWork uow, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _uow = uow;
        }
        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var gender = await _uow.UserRepository.GetUserGender(User.GetUserName());
            userParams.CurrentUserName = User.GetUserName();    
            if (string.IsNullOrEmpty(userParams.Gender))
                {
                    userParams.Gender = gender == "male" ? "female" : "male";
                }

            var users = await _uow.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(new PaginationHeader(userParams.PageNumber, userParams.PageSize, users.TotalCount, users.TotalPages));
            return Ok(users);
        }
        [HttpGet("{userName}")]
        public async Task<ActionResult<MemberDto>> GetUser(string userName)
        {
            return await _uow.UserRepository.GetMemberAsync(userName, User.GetUserName());
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await _uow.UserRepository.GetUserByUserNameAsync(User.GetUserName());

            if (user == null) return NotFound();

            _mapper.Map(memberUpdateDto, user); //map memberUpdateDto to user

            if (await _uow.Complete()) return NoContent(); //if save is successful, return no content

            return BadRequest("Failed to update user"); //if save is unsuccessful, return bad request
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _uow.UserRepository.GetUserByUserNameAsync(User.GetUserName()); //get user from repo

            if (user == null) return NotFound(); //if user is null, return not found

            var result = await _photoService.AddPhotoAsync(file); //add photo to cloudinary

            if (result.Error != null) return BadRequest(result.Error.Message); //if there is an error, return bad request

            var photo = new Photo //create new photo object
            {
                Url = result.SecureUrl.AbsoluteUri, //set url to the secure url
                PublicId = result.PublicId, //set public id to the public id
                IsApproved = false //set is approved to false
            };

            user.Photos.Add(photo); //add photo to user

            if (await _uow.Complete()) //if save is successful
            {
                return CreatedAtAction(nameof(GetUser), new { userName = user.UserName }, _mapper.Map<PhotoDto>(photo)); //return created at action
            }

            return BadRequest("Problem adding photo"); //if save is unsuccessful, return bad request

        }

         [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _uow.UserRepository.GetUserByUserNameAsync(User.GetUserName()); //get user from repo using userName from token
            
            if (user == null) return NotFound(); //if user is null, return not found

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId); //get photo from user

            if (photo == null) return NotFound(); //if photo is null, return not found

            if (photo.IsMain) return BadRequest("This is already your main photo"); //if photo is already main, return bad request

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain); //get current main photo

            if (currentMain != null) currentMain.IsMain = false; //if current main is not null, set to false

            photo.IsMain = true; //set photo to main

            if (await _uow.Complete()) return NoContent(); //if save is successful, return no content

            return BadRequest("Failed to set main photo"); //if save is unsuccessful, return bad request
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _uow.UserRepository.GetUserByUserNameAsync(User.GetUserName());

            if (user == null) return NotFound(); //if user is null, return not found

            var photo = await _uow.PhotoRepository.GetPhotoByIdAsync(photoId); //get photo from repo

            if (photo == null) return NotFound(); //if photo is null, return not found

            if (photo.IsMain) return BadRequest("You cannot delete your main photo"); //if photo is main, return bad request

            if (photo.PublicId != null) //if public id is not null
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId); //delete photo from cloudinary

                if (result.Error != null) return BadRequest(result.Error.Message); //if there is an error, return bad request
            }

            user.Photos.Remove(photo); //remove photo from user

            if (await _uow.Complete()) return Ok(); //if save is successful, return ok

            return BadRequest("Failed to delete photo"); //if save is unsuccessful, return bad request 
        }
    }
}