using AuthService.Application.Common;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Exceptions;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Sms;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Features.Otp.SendOtp;

public record SendOtpCommand(string Mobile) : IRequest<ApiResult>;

public class SendOtpCommandValidator : AbstractValidator<SendOtpCommand>
{
    public SendOtpCommandValidator()
    {
        RuleFor(x => x.Mobile)
            .NotEmpty().WithMessage("شماره موبایل الزامی است.")
            .Matches(@"^09[0-9]{9}$").WithMessage("فرمت شماره موبایل صحیح نیست.");
    }
}

public class SendOtpCommandHandler : IRequestHandler<SendOtpCommand, ApiResult>
{
    private readonly ApplicationDbContext _db;
    private readonly ISmsService          _smsService;

    public SendOtpCommandHandler(ApplicationDbContext db, ISmsService smsService)
    {
        _db         = db;
        _smsService = smsService;
    }

    public async Task<ApiResult> Handle(SendOtpCommand request, CancellationToken ct)
    {
        var userExists = await _db.Users
            .AnyAsync(u => u.Mobile == request.Mobile.Trim(), ct);

        if (!userExists)
            throw new NotFoundException("کاربری با این شماره موبایل یافت نشد.");

        var now = DateTime.UtcNow;

        var recentOtp = await _db.OtpCodes
            .Where(o => o.Mobile == request.Mobile.Trim() && !o.IsUsed && o.ExpiresAt > now)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (recentOtp is not null)
        {
            var secondsSinceLastOtp = (now - recentOtp.CreatedAt).TotalSeconds;
            if (secondsSinceLastOtp < 60)
                throw new BusinessException("لطفاً ۶۰ ثانیه صبر کنید و سپس مجدداً درخواست دهید.", "OTP_TOO_SOON");
        }

        var code = GenerateOtpCode();

        var otpCode = OtpCode.Create(request.Mobile.Trim(), code);

        _db.OtpCodes.Add(otpCode);
        await _db.SaveChangesAsync(ct);

        var sent = await _smsService.SendOtpAsync(request.Mobile.Trim(), code);

        //if (!sent)
        //    return ApiResult.Failure("خطا در ارسال پیامک. لطفاً مجدداً تلاش کنید.", 502);

        return ApiResult.Success("کد تأیید با موفقیت ارسال شد.");
    }

    private static string GenerateOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
