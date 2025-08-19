namespace GraphQLUserApi.GraphQL;

public record CreateUserInput(string Name, string Email);
public record UpdateUserInput(int Id, string Name, string Email);