using GraphQLUserApi.Data;
using GraphQLUserApi.Models;

namespace GraphQLUserApi.GraphQL;

public class Query
{
    //public IHttpContextAccessor HttpContextAccessor { get; }

    //public Query(IHttpContextAccessor httpContextAccessor)
    //{
    //    HttpContextAccessor = httpContextAccessor;
    //}
    public IQueryable<User> GetUsers([Service] AppDbContext context)
    {
        return context.Users;
    }

    public User? GetUser(int id, [Service] AppDbContext context) =>
        context.Users.FirstOrDefault(u => u.Id == id);
}
