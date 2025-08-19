using GraphQLUserApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphQLUserApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
}