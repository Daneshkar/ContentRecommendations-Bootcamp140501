using AuthService.Application.Common;
using MediatR;

namespace AuthService.Application.Features.Auth.Logout;

public record LogoutCommand : IRequest<ApiResult>;
