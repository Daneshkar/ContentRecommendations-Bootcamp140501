# مستندات پروژه Content Recommendations

> **تاریخ:** ۱۴۰۵/۰۵/۱۳  
> **ورژن دات نت:** ۸.۰  
> **معماری:** Clean Architecture + CQRS

---

## فهرست مطالب

1. [نمای کلی پروژه](#1-نمای-کلی-پروژه)
2. [معماری و Design Pattern ها](#2-معماری-و-design-pattern-ها)
3. [سرویس‌های پیاده‌سازی شده](#3-سرویس‌های-پیاده‌سازی-شده)
4. [ساختار Entity ها](#4-ساختار-entity-ها)
5. [ابزارها و کتابخانه‌ها](#5-ابزارها-و-کتابخانه‌ها)
6. [API Endpoint ها](#6-api-endpoint-ها)
7. [نکات فنی رعایت شده](#7-نکات-فنی-رعایت-شده)
8. [ایرادات و پیشنهادات بهبود](#8-ایرادات-و-پیشنهادات-بهبود)

---

## 1. نمای کلی پروژه

پروژه از **۳ اپلیکیشن میزبان** و **۹ پروژه** تشکیل شده است:

| # | پروژه | نوع | توضیح |
|---|-------|-----|-------|
| ۱ | `AuthService` | ASP.NET Core Web API | سرویس احراز هویت و مدیریت کاربران |
| ۲ | `AuthService.Application` | Class Library | لایه اپلیکیشن (CQRS Handlers) |
| ۳ | `AuthService.Domain` | Class Library | لایه دامین (Entities, Enums, Events) |
| ۴ | `AuthService.Infrastructure` | Class Library | لایه زیرساخت (EF, JWT, SMS, Middleware) |
| ۵ | `EmotionService` | ASP.NET Core Web API | سرویس مدیریت احساسات (اسکلت اولیه) |
| ۶ | `EmotionService.Application` | Class Library | لایه اپلیکیشن (فعلاً خالی) |
| ۷ | `EmotionService.Domain` | Class Library | لایه دامین (فقط Entity `Mood`) |
| ۸ | `EmotionService.Infrastructure` | Class Library | لایه زیرساخت (کپی از AuthService) |
| ۹ | `EmotionClient` | ASP.NET Core MVC | کلاینت وب (HTML/CSS/JS خام) |

### نمودار وابستگی پروژه‌ها

```
EmotionClient (MVC)
    |
    |-- HTTP -->  AuthService (Web API)
                     |
                     +-- AuthService.Application
                     |       |
                     |       +-- AuthService.Domain
                     |       +-- AuthService.Infrastructure
                     |               |
                     |               +-- AuthService.Domain
                     |
                     (EmotionClient هیچ ارجاع مستقیمی به پروژه‌های AuthService ندارد
                      و از طریق HTTP Client با API ارتباط برقرار می‌کند)

EmotionService (Web API)  ←   اسکلت اولیه، آماده توسعه
    |
    +-- EmotionService.Application  ←  فعلاً خالی
    +-- EmotionService.Domain       ←  فقط Entity Mood
    +-- EmotionService.Infrastructure ← کپی از AuthService.Infrastructure
```

---

## 2. معماری و Design Pattern ها

### 2.1 Clean Architecture (Onion Architecture)

پروژه از **Clean Architecture** با ۴ لایه پیروی می‌کند:

```
┌──────────────────────────────────────────┐
│          Presentation Layer              │
│  (AuthService/Controllers, EmotionClient)│
├──────────────────────────────────────────┤
│           Application Layer              │
│    (CQRS Handlers, Commands, Queries)    │
├──────────────────────────────────────────┤
│             Domain Layer                 │
│    (Entities, Enums, Events, Base)       │
├──────────────────────────────────────────┤
│         Infrastructure Layer             │
│   (EF Core, JWT, SMS, Guards, Pipeline)  │
└──────────────────────────────────────────┘
```

**قاعده وابستگی:** لایه‌های بیرونی به لایه‌های درونی وابسته هستند، نه برعکس. Domain به هیچ لایه‌ای وابسته نیست (جز MediatR برای `IDomainEvent : INotification`).

### 2.2 CQRS (Command Query Responsibility Segregation)

تمام عملیات‌ها از طریق **MediatR** با الگوی CQRS پیاده‌سازی شده‌اند:

- **Command** → یک `record` که `IRequest<ApiResult<T>>` را پیاده‌سازی می‌کند
- **Handler** → کلاسی که `IRequestHandler<TCommand, TResponse>` را پیاده‌سازی می‌کند
- **Validator** → اعتبارسنجی با FluentValidation

**ساختار فایل‌ها:**
```
Features/
  Auth/
    Login/        ← LoginCommand.cs (شامل Command + Response + Validator)
    Register/     ← RegisterCommand.cs
    Logout/       ← LogoutCommand.cs
    RefreshToken/ ← RefreshTokenCommand.cs
    GetProfile/   ← GetProfileQuery.cs (شامل Query + Response + Handler)
  Otp/
    SendOtp/      ← SendOtpCommand.cs (شامل Command + Validator + Handler)
    VerifyOtp/    ← VerifyOtpCommand.cs (شامل Command + Validator + Handler)
```

### 2.3 Mediator Pattern

کتابخانه **MediatR 12.4.1** به عنوان پیاده‌سازی الگوی Mediator استفاده شده است. Controller ها هیچ وابستگی مستقیمی به Handler ها ندارند و تنها از `IMediator` استفاده می‌کنند:

```csharp
// Controller - فقط یک خط
var result = await _mediator.Send(command, ct);
```

### 2.4 Pipeline Behavior (Validation Pipeline)

اعتبارسنجی خودکار از طریق **Pipeline Behavior** پیاده‌سازی شده است:

- `ValidationPipelineBehavior<TRequest, TResponse>` قبل از اجرای Handler همه Validator ها را اجرا می‌کند
- در صورت وجود خطا، `ValidationException` پرتاب می‌شود
- Controller ها نیازی به فراخوانی دستی Validator ندارند

### 2.5 Factory Method Pattern

Entity ها از **Factory Method** استاتیک برای ساخت استفاده می‌کنند (سازنده `private` است):

```csharp
// User.cs
private User() { }  // سازنده private
public static User Create(string username, string passwordHash, ...) { ... }

// OtpCode.cs
public static OtpCode Create(string mobile, string code, int expirationMinutes = 2) { ... }
```

### 2.6 Domain Events

سیستم **Domain Events** برای رویدادهای دامنه پیاده‌سازی شده:
- `IDomainEvent : INotification` (از MediatR)
- `AggregateRoot` مدیریت `_domainEvents` لیست را بر عهده دارد
- نمونه: `UserCreatedEvent` هنگام ساخت کاربر جدید اضافه می‌شود

### 2.7 Guard Pattern

کلاس `Guard` برای اعتبارسنجی‌های حفاظتی استفاده می‌شود:

```csharp
Guard.AgainstNotFound(user, "کاربر یافت نشد.");
Guard.AgainstUnauthorized(condition, "دسترسی غیرمجاز.");
Guard.AgainstNull(value, "مقدار نمی‌تواند null باشد.");
```

### 2.8 Result Pattern

کلاس‌های `ApiResult<T>` و `ApiResult` به عنوان یک Response Envelope استاندارد استفاده می‌شوند:

```json
// با Data
{ "isSuccess": true, "data": { ... }, "message": "...", "statusCode": 200 }

// بدون Data
{ "isSuccess": true, "message": "...", "statusCode": 200 }
```

### 2.9 Options Pattern

تنظیمات با استفاده از `IOptions<T>` از `appsettings.json` خوانده می‌شوند:
- `JwtSettings` → بخش `JwtSettings`
- `KavenegarSettings` → بخش `Kavenegar`

---

## 3. سرویس‌های پیاده‌سازی شده

### 3.1 AuthService — سرویس احراز هویت

**قابلیت‌ها:**

| ویژگی | توضیح |
|--------|-------|
| **ثبت‌نام** | ثبت کاربر جدید با username, password, email, mobile و سایر فیلدها |
| **ورود** | احراز هویت با username/password، تولید JWT access token و refresh token |
| **خروج** | Revoke کردن refresh token |
| **رفرش توکن** | دریافت access token جدید با refresh token |
| **پروفایل** | دریافت اطلاعات کاربر جاری (با احراز هویت) |
| **OTP موبایل** | ارسال کد تأیید ۶ رقمی از طریق Kavenegar SMS |
| **تأیید موبایل** | تأیید کد OTP و فعال‌سازی شماره موبایل کاربر |

### 3.2 EmotionService — سرویس احساسات (اسکلت اولیه)

- فاقد Controller و Handler
- فقط Entity `Mood` تعریف شده
- زیرساخت آماده (EF, JWT, Middleware) — کپی شده از AuthService

### 3.3 EmotionClient — کلاینت MVC

- صفحات Login، Register، VerifyMobile، Profile، Dashboard
- احراز هویت از طریق cookie-based JWT
- ارسال OTP و تأیید موبایل با AJAX
- طراحی RTL فارسی با HTML/CSS/JS خام (بدون Bootstrap)

---

## 4. ساختار Entity ها

### 4.1 AuthService Domain

#### User (ارث‌بری از AggregateRoot + IAuditable)

| فیلد | نوع | توضیح |
|------|-----|-------|
| `Id` | `long` | شناسه (Identity) |
| `Username` | `string` | نام کاربری (unique, max 100) |
| `FirstName` | `string?` | نام (max 100) |
| `LastName` | `string?` | نام خانوادگی (max 100) |
| `Email` | `string?` | ایمیل (unique filtered, max 255) |
| `Mobile` | `string?` | شماره موبایل (filtered index, max 20) |
| `PasswordHash` | `string` | هش رمز عبور (BCrypt) |
| `VerifyEmail` | `bool` | وضعیت تأیید ایمیل (default false) |
| `VerifyMobile` | `bool` | وضعیت تأیید موبایل (default false) |
| `AvatarUser` | `string?` | آواتار (nvarchar(max)) |
| `BirthDay` | `DateOnly?` | تاریخ تولد |
| `Gender` | `byte?` | جنسیت (۱: مرد، ۲: زن، ۳: سایر) |
| `Role` | `string` | نقش (default "User", max 50) |
| `Status` | `byte` | وضعیت (Active=۱, Inactive=۲, Banned=۳) |
| `CreatedAt` | `DateTime` | تاریخ ایجاد |
| `UpdatedAt` | `DateTime?` | تاریخ به‌روزرسانی |

**متدها:** `Create`, `IsActive`, `IsInactive`, `IsBanned`, `VerifyUserEmail`, `VerifyUserMobile`, `ChangePassword`, `UpdateAvatar`, `MakeAdmin`, `Deactivate`, `Ban`

#### RefreshToken (ارث‌بری از AggregateRoot + IAuditable)

| فیلد | نوع | توضیح |
|------|-----|-------|
| `Id` | `long` | شناسه |
| `UserId` | `long` | شناسه کاربر (FK → Users, Cascade delete) |
| `Token` | `string` | توکن (max 500, unique) |
| `ExpiresAt` | `DateTime` | زمان انقضا |
| `IsRevoked` | `bool` | وضعیت ابطال (default false) |
| `CreatedAt` | `DateTime` | تاریخ ایجاد |
| `UpdatedAt` | `DateTime?` | تاریخ به‌روزرسانی |

**متدها:** `Create`, `Revoke`  
**Properties محاسباتی:** `IsExpired`, `IsActive`

#### OtpCode (کلاس مستقل — ارث‌بری از AggregateRoot ندارد)

| فیلد | نوع | توضیح |
|------|-----|-------|
| `Id` | `long` | شناسه |
| `Mobile` | `string` | شماره موبایل (indexed, max 20) |
| `Code` | `string` | کد OTP (indexed, max 10) |
| `ExpiresAt` | `DateTime` | زمان انقضا (۲ دقیقه) |
| `IsUsed` | `bool` | وضعیت استفاده (default false) |
| `CreatedAt` | `DateTime` | تاریخ ایجاد |

**متدها:** `Create`, `IsExpired`, `MarkAsUsed`

### 4.2 EmotionService Domain

#### Mood (کلاس مستقل)

| فیلد | نوع | توضیح |
|------|-----|-------|
| `Id` | `long` | شناسه |
| `Name` | `string` | نام (unique, max 50) |
| `Description` | `string?` | توضیحات (text) |
| `IsActive` | `bool` | وضعیت فعال بودن (default true) |

**متدها:** `Create`, `Deactivate`, `Activate`

### 4.3 Enums

| Enum | مقادیر |
|------|--------|
| `UserStatus` | `Active = 1`, `Inactive = 2`, `Banned = 3` |
| `GenderType` | `Male = 1`, `Female = 2`, `Other = 3` |

### 4.4 Domain Events

| Event | توضیح |
|-------|-------|
| `UserCreatedEvent(Username)` | هنگام ساخت کاربر جدید fire می‌شود |

### 4.5 Base Classes

| کلاس | توضیح |
|------|-------|
| `AggregateRoot` | کلاس پایه برای Aggregate ها (Id, Domain Events) |
| `IAuditable` | اینترفیس برای ردیابی `CreatedAt` و `UpdatedAt` |
| `IDomainEvent : INotification` | اینترفیس رویدادهای دامنه |

---

## 5. ابزارها و کتابخانه‌ها

### 5.1 فریم‌ورک و Runtime

| ابزار | نسخه | کاربرد |
|-------|------|--------|
| **.NET** | `8.0` | Runtime |
| **ASP.NET Core** | `8.0` | Web API / MVC |

### 5.2 پایگاه داده و ORM

| ابزار | نسخه | کاربرد |
|-------|------|--------|
| **Entity Framework Core** | `8.0.8` | ORM |
| **EF Core SqlServer** | `8.0.8` | Provider SQL Server |
| **EF Core Design** | `8.0.8` | ابزارهای طراحی (Migrations) |
| **EF Core Tools** | `8.0.8` | ابزارهای CLI |

### 5.3 احراز هویت و امنیت

| ابزار | نسخه | کاربرد |
|-------|------|--------|
| **JWT Bearer** | `8.0.8` | احراز هویت JWT |
| **BCrypt.Net-Next** | `4.0.3` | هش کردن رمز عبور |

### 5.4 CQRS و Validation

| ابزار | نسخه | کاربرد |
|-------|------|--------|
| **MediatR** | `12.4.1` | پیاده‌سازی CQRS و Mediator |
| **FluentValidation** | `11.10.0` | اعتبارسنجی درخواست‌ها |
| **FluentValidation.AspNetCore** | `11.3.0` | یکپارچگی با ASP.NET Core |

### 5.5 Web API و HTTP

| ابزار | نسخه | کاربرد |
|-------|------|--------|
| **Swashbuckle** | `6.8.1` / `6.7.3` | Swagger / OpenAPI |
| **Refit** | `7.2.22` | HTTP Client خودکار (نصب شده ولی استفاده نشده) |
| **Refit.HttpClientFactory** | `7.2.22` | یکپارچگی با `IHttpClientFactory` |
| **IHttpClientFactory** | Built-in | مدیریت HttpClient با DI |

### 5.6 SMS (Kavenegar)

| پیاده‌سازی | توضیح |
|------------|-------|
| `KavenegarSmsService` | ارسال SMS از طریق REST API کاوه‌نگار با `HttpClient` |
| `ISmsService` | اینترفیس انتزاعی سرویس SMS |
| API Endpoint | `POST https://api.kavenegar.com/v1/{ApiKey}/verify/lookup.json` |
| پارامترها | `receptor` (موبایل), `token` (کد), `template` (نام قالب) |

---

## 6. API Endpoint ها

### AuthService — `https://localhost:7252/api/auth`

| # | Method | Route | Auth | Body | Response | Status Codes |
|---|--------|-------|------|------|----------|-------------|
| ۱ | `POST` | `/login` | ❌ | `{username, password}` | `ApiResult<LoginResponse>` | 200, 401, 422 |
| ۲ | `POST` | `/register` | ❌ | `{username, password, confirmPassword, ...}` | `ApiResult<RegisterResponse>` | 200, 409 |
| ۳ | `POST` | `/refresh` | ❌ | Cookie: `refresh_token` | `ApiResult<RefreshTokenResponse>` | 200, 401 |
| ۴ | `POST` | `/logout` | ✅ | — | `ApiResult` | 200 |
| ۵ | `GET` | `/profile` | ✅ | — | `ApiResult<ProfileResponse>` | 200, 401 |
| ۶ | `POST` | `/send-otp` | ❌ | `{mobile}` | `ApiResult` | 200, 404, 502 |
| ۷ | `POST` | `/verify-otp` | ❌ | `{mobile, code}` | `ApiResult` | 200, 400, 404 |

### Response Types

**`LoginResponse`:** `{ userId: long, username: string, role: string }`  
**`RegisterResponse`:** `{ userId: long, username: string }`  
**`RefreshTokenResponse`:** `{ username: string, role: string }`  
**`ProfileResponse`:** کلیه اطلاعات کاربر + وضعیت تأیید ایمیل/موبایل  

### EmotionClient — MVC Routes

| Route | Method | توضیح |
|-------|--------|-------|
| `GET/POST /Account/Login` | GET/POST | صفحه ورود |
| `GET/POST /Account/Register` | GET/POST | صفحه ثبت‌نام |
| `GET /Account/VerifyMobile` | GET | صفحه فعال‌سازی موبایل پس از ثبت‌نام |
| `GET /Account/Profile` | GET | صفحه پروفایل کاربر |
| `POST /Account/Logout` | POST | خروج از حساب |
| `POST /send-otp-ajax` | POST | ارسال OTP (AJAX) |
| `POST /verify-otp-ajax` | POST | تأیید OTP (AJAX) |
| `GET /Home/Index` | GET | داشبورد |

---

## 7. نکات فنی رعایت شده

### 7.1 معماری

- ✅ **Clean Architecture** — جداسازی کامل لایه‌های Domain, Application, Infrastructure, Presentation
- ✅ **CQRS** — تفکیک خواندن و نوشتن با MediatR
- ✅ **Separation of Concerns** — هر لایه مسئولیت مشخصی دارد
- ✅ **Dependency Inversion** — لایه‌های داخلی هیچ وابستگی به لایه‌های خارجی ندارند

### 7.2 امنیت

- ✅ **BCrypt** — هش کردن رمز عبور با BCrypt.Net (الگوریتم مقاوم در برابر brute-force)
- ✅ **JWT Bearer** — احراز هویت مبتنی بر JWT با HS256
- ✅ **HttpOnly Cookies** — توکن‌ها در کوکی‌های HttpOnly و Secure ذخیره می‌شوند (غیرقابل دسترسی از JavaScript)
- ✅ **Refresh Token Rotation** — توکن‌های refresh قدیمی در هر لاگین جدید Revoke می‌شوند
- ✅ **Password Validation** — اعتبارسنجی طول رمز عبور (حداقل ۶ کاراکتر)
- ✅ **Input Validation** — تمام ورودی‌ها با FluentValidation اعتبارسنجی می‌شوند

### 7.3 کیفیت کد

- ✅ **Immutability** — Entity ها با `private set` و Factory Method طراحی شده‌اند
- ✅ **Rich Domain Model** — منطق کسب‌وکار در خود Entity ها قرار دارد (نه در سرویس‌ها)
- ✅ **Result Pattern** — تمام پاسخ‌ها با یک envelope استاندارد (`ApiResult<T>`) برگردانده می‌شوند
- ✅ **Guard Clauses** — اعتبارسنجی‌های حفاظتی با الگوی Guard
- ✅ **Null-safety** — استفاده از Nullable Reference Types فعال در تمام پروژه‌ها

### 7.4 مدیریت خطا

- ✅ **Global Exception Handling** — Middleware سراسری مدیریت خطاها
- ✅ **Business vs Technical Errors** — تفکیک خطاهای کسب‌وکار (`BusinessException`) از خطاهای فنی
- ✅ **Persian Error Messages** — پیام‌های خطا به فارسی
- ✅ **Structured Error Response** — پاسخ خطا با `ErrorResponse` شامل status, type, message, errorCode, traceId

### 7.5 مدیریت OTP

- ✅ **Rate Limiting** — محدودیت ۶۰ ثانیه بین درخواست‌های OTP
- ✅ **Expiration** — کد OTP پس از ۲ دقیقه منقضی می‌شود
- ✅ **One-Time Use** — هر کد OTP فقط یک بار قابل استفاده است
- ✅ **6-Digit Random** — کد ۶ رقمی تصادفی (100000 تا 999999)
- ✅ **Server-Side Validation** — اعتبارسنجی کامل OTP در سمت سرور

### 7.6 Frontend (EmotionClient)

- ✅ **Vanilla JS** — بدون وابستگی به jQuery یا Bootstrap
- ✅ **RTL Support** — طراحی کاملاً راست‌چین فارسی
- ✅ **Progressive Enhancement** — کارکرد حتی بدون JavaScript
- ✅ **CSRF Protection** — فرم‌ها با ASP.NET Core Anti-Forgery محافظت می‌شوند
- ✅ **Otp Input UX** — پشتیبانی از paste، auto-focus بین فیلدها، تایمر شمارش معکوس

### 7.7 DevOps و Deployment

- ✅ **Auto-Migrations** — اجرای خودکار مایگریشن‌ها در startup
- ✅ **Environment-based Config** — تنظیمات در `appsettings.json` و `appsettings.Development.json`
- ✅ **Swagger** — مستندات API در محیط Development

---

## 8. ایرادات و پیشنهادات بهبود

### 8.1 ایرادات معماری

| # | مشکل | شدت | توضیح | پیشنهاد |
|---|-------|-----|-------|---------|
| ۱ | **کد تکراری در EmotionService** | متوسط | لایه Infrastructure در EmotionService تقریباً کپی کامل از AuthService است. Exception ها، Middleware، JwtSettings و Pipeline Behavior دقیقاً تکرار شده‌اند. | استخراج کد مشترک به یک پروژه `Shared` یا `Common` جداگانه |
| ۲ | **OtpCode از AggregateRoot ارث‌بری ندارد** | پایین | `OtpCode` برخلاف `User` و `RefreshToken` از `AggregateRoot` ارث‌بری نمی‌کند. این ناهماهنگی در طراحی دامنه است. | یا همه entity ها باید از AggregateRoot ارث‌بری کنند یا یک Base class مشترک داشته باشند |
| ۳ | **نبود MediatR Registration در EmotionService** | متوسط | در `Program.cs` مربوط به EmotionService فراخوانی `AddMediatR` و `AddValidatorsFromAssembly` وجود ندارد | اضافه کردن ثبت خودکار MediatR برای Handler های آینده |

### 8.2 ایرادات امنیتی

| # | مشکل | شدت | توضیح | پیشنهاد |
|---|-------|-----|-------|---------|
| ۴ | **JwtSettings در appsettings.json افشا شده** | بالا | کلید `SecretKey` در `appsettings.json` به صورت plain text ذخیره شده | استفاده از User Secrets در توسعه و Azure Key Vault / Environment Variables در production |
| ۵ | **Kavenegar ApiKey در appsettings.json** | بالا | توکن کاوه‌نگار در `appsettings.json` ذخیره شده | انتقال به User Secrets / Environment Variables |
| ۶ | **عدم Rate Limiting سراسری** | متوسط | هیچ مکانیزم rate limiting برای جلوگیری از brute-force روی endpoint های login و send-otp وجود ندارد | اضافه کردن `AddRateLimiter` در Program.cs یا استفاده از middleware مخصوص |
| ۷ | **SendOtp بدون احراز هویت** | متوسط | هر کسی می‌تواند بدون لاگین برای هر شماره‌ای OTP درخواست دهد (هرچند کاربر باید وجود داشته باشد) | اضافه کردن rate limit سخت‌گیرانه‌تر برای این endpoint |

### 8.3 ایرادات عملکردی

| # | مشکل | شدت | توضیح | پیشنهاد |
|---|-------|-----|-------|---------|
| ۸ | **Email Verification پیاده‌سازی نشده** | متوسط | `VerifyEmail` و `VerifyUserEmail()` در دامنه وجود دارند اما هیچ endpoint یا سرویسی برای تأیید ایمیل پیاده‌سازی نشده | اضافه کردن سرویس ایمیل و endpoint تأیید ایمیل |
| ۹ | **امکان ثبت‌نام بدون موبایل یا ایمیل** | پایین | کاربر می‌تواند بدون هیچ راه ارتباطی (بدون email و mobile) ثبت‌نام کند | اضافه کردن اعتبارسنجی که حداقل یکی از email یا mobile اجباری باشد |
| ۱۰ | **تاریخ تولد در ثبت‌نام دریافت نمی‌شود** | پایین | `User.Create` پارامتر `birthDay` را می‌پذیرد اما `RegisterCommandHandler` آن را ارسال نمی‌کند | اضافه کردن فیلد `BirthDay` به `RegisterCommand` |

### 8.4 ایرادات کیفیت کد

| # | مشکل | شدت | توضیح | پیشنهاد |
|---|-------|-----|-------|---------|
| ۱۱ | **عدم یکپارچگی Exception ها بین دو سرویس** | کم | `EmotionService` کلاس‌های `UnauthorizedException` و `ForbiddenException` را ندارد، درحالیکه `AuthService` دارد | یکسان‌سازی Exception ها در یک پروژه مشترک |
| ۱۲ | **نبود JwtService در EmotionService** | کم | `EmotionService` فقط `JwtSettings` دارد اما `JwtService` ندارد. این یعنی نمی‌تواند توکن تولید کند | یا JwtService اضافه شود یا از AuthService حذف شود |
| ۱۳ | **استفاده از `new Random()` در Handler** | کم | در `SendOtpCommandHandler` هر بار یک `new Random()` ساخته می‌شود که seed یکسانی در فراخوانی‌های سریع خواهد داشت | استفاده از `Random.Shared` یا `RandomNumberGenerator` |
| ۱۴ | **دریافت access_token از Cookie در EmotionClient** | متوسط | `EmotionClient` توکن را از cookie می‌خواند اما این cookie با HttpOnly تنظیم شده — در واقع از `Response.Cookies` خودش می‌خواند (که از AuthService فوروارد شده). این منطق شکننده است | استفاده از Session یا Token Store سمت سرور |
| ۱۵ | **ساخت HttpClient دستی برای Logout** | پایین | در `AccountController.Logout` یک `HttpClient` جدید با `using` ساخته می‌شود که استفاده از `IHttpClientFactory` توصیه می‌شود | انتقال logic logout به `AuthApiService` |

### 8.5 ایرادات Domain Design

| # | مشکل | شدت | توضیح | پیشنهاد |
|---|-------|-----|-------|---------|
| ۱۶ | **Role به صورت string ذخیره می‌شود** | کم | نقش کاربر به جای enum به صورت string ذخیره شده ("User", "Admin") | استفاده از Enum برای Role‌ها |
| ۱۷ | **Status به صورت byte ذخیره می‌شود** | کم | وضعیت کاربر با `byte` ذخیره شده و cast به `UserStatus` می‌شود. این باعث کاهش خوانایی کد می‌شود | استفاده از `Value Conversion` در EF Core برای نگاشت خودکار |
| ۱۸ | **Gender به صورت byte ذخیره می‌شود** | کم | مشابه Status، جنسیت هم با `byte` ذخیره می‌شود | استفاده از Enum + Value Conversion |

### 8.6 پیشنهادات بهبود

| # | پیشنهاد | اولویت |
|---|---------|--------|
| ۱ | **افزودن Integration Tests** برای Handler ها و سرویس SMS | بالا |
| ۲ | **رفرش خودکار توکن** در EmotionClient قبل از expire شدن | متوسط |
| ۳ | **Retry Policy** برای فراخوانی Kavenegar API در صورت خطای شبکه | متوسط |
| ۴ | **Background Job** برای پاکسازی خودکار OtpCode های منقضی شده | پایین |
| ۵ | **ایجاد Shared Kernel** برای کدهای مشترک بین دو میکروسرویس (ApiResult, Exception ها, Middleware) | متوسط |
| ۶ | **Health Check Endpoint** برای هر دو سرویس | متوسط |
| ۷ | **Structured Logging** با Serilog | پایین |
| ۸ | **API Versioning** برای endpoint ها | پایین |

---

## ضمیمه: تنظیمات مورد نیاز

### appsettings.json — AuthService

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost;Initial Catalog=EmotionServiceDb;..."
  },
  "JwtSettings": {
    "SecretKey": "کلید-مخفی-حداقل-۳۲-کاراکتر",
    "Issuer": "AuthService",
    "Audience": "EmotionApp",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 7
  },
  "Kavenegar": {
    "ApiKey": "توکن-دریافتی-از-کاوه-نگار",
    "Sender": "10008663",
    "Template": "verify"
  }
}
```

### appsettings.json — EmotionClient

```json
{
  "AuthService": {
    "BaseUrl": "https://localhost:7252"
  }
}
```

---

> **نکته:** این مستندات بر اساس وضعیت فعلی پروژه در تاریخ ۱۴۰۵/۰۵/۱۳ تهیه شده و ممکن است با توسعه‌های آتی تغییر کند.
