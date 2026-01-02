using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Models.Interfaces;

namespace Models.Authorization;

public class FriendAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, IFriend>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        IFriend resource)
    {
        // Simple policy:
        // - Anonymous users can only Read
        // - Authenticated users can Create/Read/Edit/Delete
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            if (requirement.Name == CrudOperations.Read.Name)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }

        if (requirement.Name == CrudOperations.Create.Name
            || requirement.Name == CrudOperations.Read.Name
            || requirement.Name == CrudOperations.Edit.Name
            || requirement.Name == CrudOperations.Delete.Name)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
