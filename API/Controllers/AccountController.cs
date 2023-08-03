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

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            _mapper = mapper;
            _tokenService = tokenService;
            _context = context;
        }
        
        [HttpPost("register")] // POST: api/account/register?userName=abc&password=123
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            //check if userName already exists
            if (await UserExists(registerDto.UserName)) return BadRequest("UserName is taken");

            var user = _mapper.Map<AppUser>(registerDto);

            using var hmac = new HMACSHA512();

            user.UserName = registerDto.UserName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
                //we cant return photo of a new user because they dont have a photo yet
            };
        }

        [HttpPost("login")] // POST: api/account/login
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            
            //get user from db
            var user = await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(AppUser => AppUser.UserName == loginDto.UserName.ToLower());

            //if null, return unauthorized
            if (user == null) return Unauthorized("Invalid userName");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            //compare computedHash with PasswordHash
            for (int i = 0; i < computedHash.Length; i++)
            {
                //if not equal, return unauthorized
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            return new UserDto
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        private async Task<bool> UserExists(string userName)
        {
            return await _context.Users.AnyAsync(AppUser => AppUser.UserName == userName.ToLower());
        }
    }
}