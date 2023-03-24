﻿using BCrypt.Net;
using JinEventsWebAPI.Models;
using JinEventsWebAPI.Models.Dto;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JinEventsWebAPI.Controllers
{
	[EnableCors("Allow all")]
	[Route("api/auth")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly JinEventsContext _context;
		private readonly IConfiguration _configuration;

		public AuthController(JinEventsContext context, IConfiguration configuration)
		{
			_context = context;
			_configuration = configuration;
		}

		[HttpPost("register")]
		public ActionResult<User> Register(UserRegiserDto userDto)
		{
			string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
			HashType ht = HashType.SHA512;
			string HashedPass = BCrypt.Net.BCrypt.HashPassword(userDto.Password, salt, true, ht);

			User user = new()
			{
				Email = userDto.Email,
				Login = userDto.UserName,
				Password = HashedPass,
				Created = DateTime.Now.ToUniversalTime(),
				Rating = decimal.Zero,
			};

			try
			{
				_context.Users.Add(user);
				_context.SaveChanges();
				return Ok(user);
			}
			catch (Exception)
			{
				return BadRequest(user);
			}
		}

		[HttpPost("login")]
		public ActionResult<User> Login(UserLoginDto userDto)
		{
			try
			{
				var user = _context.Users.FirstOrDefault(u => u.Login == userDto.UserName);

				if (user != null)
				{
					if (!BCrypt.Net.BCrypt.Verify(userDto.Password, user.Password))
					{
						return BadRequest();
					}
					string userToken = GenerateToken(user);
					return Ok(userToken);
				}
				else
				{
					return NotFound();
				}
			}
			catch (Exception)
			{
				return BadRequest();
			}
		}

		private string GenerateToken(User user)
		{
			try
			{
				if(user.Login != null)
				{
					List<Claim> claims = new()
					{
						new Claim(ClaimTypes.Sid, user.Id.ToString()),
						new Claim(ClaimTypes.Email, user.Email),
						new Claim(ClaimTypes.UserData, user.Login),
						new Claim(ClaimTypes.UserData, user.Password),
						new Claim(ClaimTypes.UserData, user.ProjectMembers.Count.ToString()),
						new Claim(ClaimTypes.UserData, user.Comments.Count.ToString()),
						new Claim(ClaimTypes.UserData, user.Rating.ToString()),
					};
				
					var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("SecurityKeys:Token").Value!));

					var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

					var token = new JwtSecurityToken(
							claims: claims,
							expires: DateTime.Now.AddHours(3),
							signingCredentials: creds
						);
				
					var jwt = new JwtSecurityTokenHandler().WriteToken(token);
				
					return jwt;
				}

				return "Can`t create token on wrong data";
			}
			catch (Exception)
			{
				return "Token generation not working";
			}
		}
	}
}
