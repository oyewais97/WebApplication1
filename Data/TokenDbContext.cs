using Microsoft.EntityFrameworkCore;
using WebApplication1.Model;
using static WebApplication1.Model.Token;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApplication1.Data;
using Microsoft.EntityFrameworkCore;

public class TokenDbContext : DbContext
{
    public DbSet<Token> Tokens { get; set; }

    public TokenDbContext(DbContextOptions<TokenDbContext> options) : base(options)
    {
    }
}
