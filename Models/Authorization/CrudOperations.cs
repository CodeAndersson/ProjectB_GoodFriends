using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Models.Authorization;

public static class CrudOperations
{
    public static OperationAuthorizationRequirement Create { get; } = new() { Name = nameof(Create) };
    public static OperationAuthorizationRequirement Read { get; } = new() { Name = nameof(Read) };
    public static OperationAuthorizationRequirement Edit { get; } = new() { Name = nameof(Edit) };
    public static OperationAuthorizationRequirement Delete { get; } = new() { Name = nameof(Delete) };
}
