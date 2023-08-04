using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
        }
        
        [HttpPost("register")] // POST: api/account/register?userName=abc&password=123
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            //check if userName already exists
            if (await UserExists(registerDto.UserName)) return BadRequest("UserName is taken");

            var user = _mapper.Map<AppUser>(registerDto);

            user.UserName = registerDto.UserName.ToLower();

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded) return BadRequest(result.Errors);

            return new UserDto
            {
                UserName = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
                //we cant return photo of a new user because they dont have a photo yet
            };
        }

        [HttpPost("login")] // POST: api/account/login
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            
            //get user from db
            var user = await _userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(AppUser => AppUser.UserName == loginDto.UserName.ToLower());

            //if null, return unauthorized
            if (user == null) return Unauthorized("Invalid userName");

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!result) return Unauthorized();

            return new UserDto
            {
                UserName = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
            

        }

        private async Task<bool> UserExists(string userName)
        {
            return await _userManager.Users.AnyAsync(AppUser => AppUser.UserName == userName.ToLower());
        }
    }
}