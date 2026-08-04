using AuthService.Application.Common;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Exceptions;
using AuthService.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Auth.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResult<RegisterResponse>>
{
    private readonly ApplicationDbContext _db;

    public RegisterCommandHandler(ApplicationDbContext db)
        => _db = db;

    public async Task<ApiResult<RegisterResponse>> Handle(
        RegisterCommand   request,
        CancellationToken ct)
    {
        var usernameExists = await _db.Users
            .AnyAsync(u => u.Username == request.Username.Trim().ToLower(), ct);

        if (usernameExists)
            throw new ConflictException("این نام کاربری قبلاً ثبت شده است.");

        if (!string.IsNullOrEmpty(request.Email))
        {
            var emailExists = await _db.Users
                .AnyAsync(u => u.Email == request.Email.Trim().ToLower(), ct);

            if (emailExists)
                throw new ConflictException("این ایمیل قبلاً ثبت شده است.");
        }

        if (!string.IsNullOrEmpty(request.Mobile))
        {
            var mobileExists = await _db.Users
                .AnyAsync(u => u.Mobile == request.Mobile.Trim(), ct);

            if (mobileExists)
                throw new ConflictException("این شماره موبایل قبلاً ثبت شده است.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = User.Create(
            username:     request.Username,
            passwordHash: passwordHash,
            email:        request.Email,
            mobile:       request.Mobile,
            firstName:    request.FirstName,
            lastName:     request.LastName,
            birthDay:     request.BirthDay,
            gender:       request.Gender);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return ApiResult<RegisterResponse>.Success(
            new RegisterResponse(user.Id, user.Username),
            "ثبت‌نام با موفقیت انجام شد.");
    }
}
