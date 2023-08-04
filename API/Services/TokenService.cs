using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<AppUser> _userManager;
        public TokenService(IConfiguration config, UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])); 
        }
        public async Task<string> CreateToken(AppUser user)
        {
            var claims = new List<Claim>
            {
                //add userName to token
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)

            };

            var roles = await _userManager.GetRolesAsync(user); //get roles of user

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role))); //add roles to token

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature); //create signing credentials

            var tokenDescriptor = new SecurityTokenDescriptor //create token descriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7), //token expires in 7 days
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler(); //create token handler

            var token = tokenHandler.CreateToken(tokenDescriptor); //create token

            return tokenHandler.WriteToken(token); //return token
        }
    }
}