using GraphQLUserApi.Data;
using GraphQLUserApi.Models;

namespace GraphQLUserApi.GraphQL;

public class Mutation
{
    public async Task<string> CreateUsers(List<CreateUserInput> inputs, [Service] AppDbContext context)
    {
        var users = inputs.Select(input => new User
        {
            Name = input.Name,
            Email = input.Email
        }).ToList();

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
        return "Users created successfully";
    }

    public async Task<User?> UpdateUser(UpdateUserInput input, [Service] AppDbContext context)
    {
        var user = await context.Users.FindAsync(input.Id);
        if (user == null) return null;

        user.Name = input.Name;
        user.Email = input.Email;
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUser(int id, [Service] AppDbContext context)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null) return false;

        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return true;
    }
}
