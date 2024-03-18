using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Collections.Generic;
using WebApplication1.Data;
using WebApplication1.Model;
using NBitcoin.Secp256k1;

public class TokenController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly TokenDbContext _context;

    public TokenController(TokenDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        var user = _context.user.SingleOrDefault(u => u.Username == model.Username);
        if (user != null && user.Password == model.Password)
        {
            var token = GenerateJwtToken();
            return Ok(new { token });

        }
        return Unauthorized();

    }

    private string GenerateJwtToken()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:secret"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "user"),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(30),
            SigningCredentials = credentials,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [HttpGet("users")]
    public IActionResult GetAllUsers()
    {
        var users = _context.user.ToList();
        return Ok(users);
    }

    [Authorize]
    [HttpPost("calculate-supply")]
    public IActionResult CalculateSupply([FromBody] CalculateSupplyModel model)
    {
        if (model == null)
        {
            return BadRequest("Invalid request");
        }

        // Get the total supply from the model
        decimal totalSupply = model.TotalSupply;

        // Hardcoded non-circulating addresses
        var nonCirculatingAddresses = new[]
        {
                "0x000000000000000000000000000000000000dEaD",
                "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56",
                "0xfE1d7f7a8f0bdA6E415593a2e4F82c64b446d404",
                "0x71F36803139caC2796Db65F373Fb7f3ee0bf3bF9",
                "0x62D6d26F86F2C1fBb65c0566Dd6545ae3F9A63F1",
                "0x83a7152317DCfd08Be0F673Ab614261b4D1e1622",
                "0x5A749B82a55f7d2aCEc1d71011442E221f55A537",
                "0x9eBbBE47def2F776D6d2244AcB093AB2fD1B2C2A",
                "0xcdD80c6F317898a8aAf0ec7A664655E25E4833a2",
                "0x456F20bb4d89d10A924CE81b7f0C89D5711CE05B"
            };

        // Calculate non-circulating supply
        decimal nonCirculatingSupply = 0;
        foreach (var address in nonCirculatingAddresses)
        {
            // Query the database for token amounts associated with each address
            var tokenAmount = _context.Token
                .Where(t => t.Address == address)
                .Sum(t => t.totalAmount); ;

            nonCirculatingSupply += tokenAmount;
        }

        // Calculate circulating supply
        decimal circulatingSupply = totalSupply - nonCirculatingSupply;

        // Return supply data
        return Ok(new
        {
            TotalSupply = totalSupply,
            CirculatingSupply = circulatingSupply
        });
    }

    [Authorize]
    [HttpPost("update-token")]
    public IActionResult UpdateToken([FromBody] UpdateTokenModel model)
    {
        try
        {
            // Retrieve the token entity from the database based on the provided address
            var token = _context.Token.FirstOrDefault(t => t.Address == model.Address);

            if (token == null)
            {
                return NotFound("Token not found");
            }

            // Update the TotalSupply
            token.TotalSupply = model.TotalSupply;

            // Save changes to the database
            _context.SaveChanges();

            return Ok("Token updated successfully");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating token: {ex.Message}");
        }
    }
    [HttpGet("token-data")]
    public IActionResult GetTokenData()
    {
        try
        {
            // Retrieve token data from the database
            var tokenData = _context.Token.Select(t => new
            {
                t.Id,
                t.Name,
                TotalSupply = t.TotalSupply , // Handle NULL TotalSupply
                CirculatingSupply = t.CirculatingSupply, // Handle NULL CirculatingSupply
                t.Address,
                TotalAmount = t.totalAmount  // Handle NULL TotalAmount
            }).ToList();

            return Ok(tokenData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error fetching token data: {ex.Message}");
        }
    }
}
    /* [HttpPost("calculate-supply")]
     public IActionResult CalculateSupply([FromBody] CalculateSupplyModel model)
     {
         if (model == null)
         {
             return BadRequest("Invalid request");
         }


         decimal totalSupply = model.TotalSupply;


         var nonCirculatingAddresses = new[]
         {
             "0x000000000000000000000000000000000000dEaD",
             "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56",
             "0xfE1d7f7a8f0bdA6E415593a2e4F82c64b446d404",
             "0x71F36803139caC2796Db65F373Fb7f3ee0bf3bF9",
             "0x62D6d26F86F2C1fBb65c0566Dd6545ae3F9A63F1",
             "0x83a7152317DCfd08Be0F673Ab614261b4D1e1622",
             "0x5A749B82a55f7d2aCEc1d71011442E221f55A537",
             "0x9eBbBE47def2F776D6d2244AcB093AB2fD1B2C2A",
             "0xcdD80c6F317898a8aAf0ec7A664655E25E4833a2",
             "0x456F20bb4d89d10A924CE81b7f0C89D5711CE05B"
         };


         decimal nonCirculatingSupply = nonCirculatingAddresses.Sum(address =>
         {

             decimal tokenAmount = 1000;
             return tokenAmount;
         });

         // Calculate circulating supply
         decimal circulatingSupply = totalSupply - nonCirculatingSupply;

         return Ok(new
         {
             TotalSupply = totalSupply,
             CirculatingSupply = circulatingSupply
         });
     }


    [HttpGet("token-data")]
     public IActionResult GetTokenData()
     {
         try
         {

             var tokenData = _context.Tokens.FirstOrDefault();
             if (tokenData == null)
             {
                 return NotFound("Token data not found");
             }

             // Return token data
             return Ok(tokenData);
         }
         catch (Exception ex)
         {
             return StatusCode(500, $"Error fetching token data: {ex.Message}");
         }
     }
    }*/

    public class LoginModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class CalculateSupplyModel
{
    public decimal TotalSupply { get; set; }
}

public class Token
{
    public int Id { get; set; }
    public string Name { get; set; }

    public string Address { get; set; }
    public decimal TotalSupply { get; set; }
    public decimal CirculatingSupply { get; set; }

    public decimal totalAmount { get; set; }
        // Add other token properties as needed
    }
public class UpdateTokenModel
{
    public string Address { get; set; }
    public decimal TotalSupply { get; set; }
}
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    // Other user properties
}

public class TokenDbContext : DbContext
{
    public DbSet<Token> Token { get; set; }
    public DbSet<User> user { get; set; }

    public TokenDbContext(DbContextOptions<TokenDbContext> options) : base(options)
    {
    }
}
