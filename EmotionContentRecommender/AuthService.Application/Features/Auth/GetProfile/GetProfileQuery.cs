using AuthService.Application.Common;
using AuthService.Infrastructure.Extensions;
using AuthService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Auth.GetProfile;

public record GetProfileQuery : IRequest<ApiResult<ProfileResponse>>;

public record ProfileResponse(
    long     UserId,
    string   Username,
    string?  FirstName,
    string?  LastName,
    string?  Email,
    bool     VerifyEmail,
    string?  Mobile,
    bool     VerifyMobile,
    byte?    Gender,
    DateOnly? BirthDay,
    string?  AvatarUser,
    string   Role);

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ApiResult<ProfileResponse>>
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService  _currentUser;

    public GetProfileQueryHandler(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db          = db;
        _currentUser = currentUser;
    }

    public async Task<ApiResult<ProfileResponse>> Handle(GetProfileQuery request, CancellationToken ct)
    {
        var userId = _currentUser.GetUserId();

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return ApiResult<ProfileResponse>.NotFound("کاربر یافت نشد.");

        var profile = new ProfileResponse(
            UserId:       user.Id,
            Username:     user.Username,
            FirstName:    user.FirstName,
            LastName:     user.LastName,
            Email:        user.Email,
            VerifyEmail:  user.VerifyEmail,
            Mobile:       user.Mobile,
            VerifyMobile: user.VerifyMobile,
            Gender:       (byte?)user.Gender,
            BirthDay:     user.BirthDay,
            AvatarUser:   user.AvatarUser,
            Role:         user.Role.ToString());

        return ApiResult<ProfileResponse>.Success(profile, "اطلاعات پروفایل با موفقیت دریافت شد.");
    }
}
