using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Db;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : Controller
{
    IConfiguration _config;

    public UsersController(IConfiguration config)
    {
        _config = config;
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        AppSQLDb _db = new AppSQLDb(_config["ConnectionStrings:DefaultConnection"]);
        await _db.Connection.OpenAsync();

        using var cmd = _db.Connection.CreateCommand();
        var sql = "SELECT * FROM [User] WHERE id=" + id;
        cmd.CommandText = sql;
        var reader = await cmd.ExecuteReaderAsync();
        var user = new User();

        using (reader)
        {
            await reader.ReadAsync();
            user.Id = reader.GetInt32("id");
            user.Name = reader.GetString("name");
            user.Email = reader.GetString("email");
            user.Username = reader.GetString("username");
            user.Password = reader.GetString("password");
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Authenticate([FromBody] LoginForm form)
    {
        AppSQLDb _db = new AppSQLDb(_config["ConnectionStrings:DefaultConnection"]);
        await _db.Connection.OpenAsync();

        using var cmd = _db.Connection.CreateCommand();
        var sql = "SELECT * FROM [User] WHERE username='" + form.Username + "'"
                        + " AND password='" + form.Password + "'";
        cmd.CommandText = sql;
        var reader = await cmd.ExecuteReaderAsync();
        var user = new User();

        if (reader.HasRows)
        {
            var authClaims = new List<Claim>
            {
                new(ClaimTypes.Name, form.Username),
                new(ClaimTypes.Role, "ADMIN")
            };

            var key = _config["Jwt:Key"];
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            using (reader)
            {
                await reader.ReadAsync();
                user.Id = reader.GetInt32("id");
                user.Name = reader.GetString("name");
                user.Email = reader.GetString("email");
                user.Username = reader.GetString("username");
                user.Password = reader.GetString("password");
                user.Token = new JwtSecurityTokenHandler().WriteToken(token);
            }
        }

        return Ok(user);
    }

}