using AuthService.Application.Common;
using AuthService.Infrastructure.Exceptions;
using AuthService.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Otp.VerifyOtp;

public record VerifyOtpCommand(string Mobile, string Code) : IRequest<ApiResult>;

public class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.Mobile)
            .NotEmpty().WithMessage("شماره موبایل الزامی است.")
            .Matches(@"^09[0-9]{9}$").WithMessage("فرمت شماره موبایل صحیح نیست.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("کد تأیید الزامی است.")
            .Length(6).WithMessage("کد تأیید باید ۶ رقمی باشد.");
    }
}

public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, ApiResult>
{
    private readonly ApplicationDbContext _db;

    public VerifyOtpCommandHandler(ApplicationDbContext db) => _db = db;

    public async Task<ApiResult> Handle(VerifyOtpCommand request, CancellationToken ct)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Mobile == request.Mobile.Trim(), ct);

        if (user is null)
            throw new NotFoundException("کاربری با این شماره موبایل یافت نشد.");

        var otpCode = await _db.OtpCodes
            .Where(o => o.Mobile == request.Mobile.Trim()
                        && o.Code == request.Code.Trim()
                        && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (otpCode is null)
            throw new BusinessException("کد تأیید نامعتبر است.", "INVALID_OTP");

        if (otpCode.IsExpired())
            throw new BusinessException("کد تأیید منقضی شده است. لطفاً مجدداً درخواست دهید.", "OTP_EXPIRED");

        otpCode.MarkAsUsed();
        user.VerifyUserMobile();

        await _db.SaveChangesAsync(ct);

        return ApiResult.Success("شماره موبایل با موفقیت تأیید شد.");
    }
}
