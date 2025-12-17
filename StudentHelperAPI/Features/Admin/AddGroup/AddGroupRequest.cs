using MediatR;

namespace StudentHelperAPI.Features.Admin.AddGroup
{
    public record AddGroupRequest(string Name) : IRequest<AddGroupResponse>;
}
