function setupOtpInput() {
    const inputs = document.querySelectorAll('.otp-input-row input');
    if (inputs.length === 0) return;

    inputs.forEach((input, index) => {
        input.addEventListener('input', (e) => {
            if (e.target.value.length === 1 && index < inputs.length - 1) {
                inputs[index + 1].focus();
            }
            checkOtpComplete();
        });

        input.addEventListener('keydown', (e) => {
            if (e.key === 'Backspace' && !e.target.value && index > 0) {
                inputs[index - 1].focus();
            }
        });

        input.addEventListener('paste', (e) => {
            e.preventDefault();
            const paste = (e.clipboardData || window.clipboardData).getData('text');
            const digits = paste.replace(/\D/g, '').slice(0, 6).split('');
            inputs.forEach((inp, i) => {
                if (digits[i]) inp.value = digits[i];
            });
            if (digits.length > 0) {
                const lastIndex = Math.min(digits.length, inputs.length) - 1;
                inputs[lastIndex].focus();
            }
            checkOtpComplete();
        });
    });
}

function checkOtpComplete() {
    const inputs = document.querySelectorAll('.otp-input-row input');
    let code = '';
    inputs.forEach(i => code += i.value);
    if (code.length === 6) {
        document.getElementById('otp-code-hidden').value = code;
    }
}

function startTimer(durationSeconds) {
    const el = document.getElementById('otp-timer');
    const resend = document.getElementById('resend-link');
    let remaining = durationSeconds;

    const update = () => {
        const m = Math.floor(remaining / 60);
        const s = remaining % 60;
        el.textContent = `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
        if (remaining <= 0) {
            clearInterval(timer);
            el.textContent = '';
            if (resend) resend.classList.remove('disabled');
            return;
        }
        remaining--;
    };

    update();
    const timer = setInterval(update, 1000);
}

let otpSent = false;

function sendOtp(mobile) {
    if (otpSent) return;
    const btn = document.getElementById('send-otp-btn');
    const spinner = document.getElementById('otp-spinner');
    const msg = document.getElementById('otp-message');
    const section = document.getElementById('otp-section');

    btn.disabled = true;
    spinner.style.display = 'block';

    fetch('/send-otp-ajax', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ mobile: mobile })
    })
    .then(r => r.json())
    .then(data => {
        spinner.style.display = 'none';
        if (data.isSuccess) {
            otpSent = true;
            section.style.display = 'block';
            msg.innerHTML = '<div class="alert alert-success">کد تأیید ارسال شد</div>';
            startTimer(120);
            setupOtpInput();
        } else {
            btn.disabled = false;
            msg.innerHTML = '<div class="alert alert-error">' + (data.message || 'خطا در ارسال') + '</div>';
        }
    })
    .catch(() => {
        spinner.style.display = 'none';
        btn.disabled = false;
        msg.innerHTML = '<div class="alert alert-error">خطا در ارتباط با سرور</div>';
    });
}

function verifyOtp(mobile) {
    const code = document.getElementById('otp-code-hidden').value;
    if (code.length !== 6) return;

    const btn = document.getElementById('verify-otp-btn');
    const spinner = document.getElementById('verify-spinner');
    const msg = document.getElementById('otp-verify-message');

    btn.disabled = true;
    spinner.style.display = 'block';

    fetch('/verify-otp-ajax', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ mobile: mobile, code: code })
    })
    .then(r => r.json())
    .then(data => {
        spinner.style.display = 'none';
        if (data.isSuccess) {
            msg.innerHTML = '<div class="alert alert-success">شماره موبایل با موفقیت تأیید شد. در حال انتقال...</div>';
            setTimeout(() => {
                window.location.href = '/Account/Login';
            }, 1500);
        } else {
            btn.disabled = false;
            msg.innerHTML = '<div class="alert alert-error">' + (data.message || 'کد نامعتبر') + '</div>';
        }
    })
    .catch(() => {
        spinner.style.display = 'none';
        btn.disabled = false;
        msg.innerHTML = '<div class="alert alert-error">خطا در ارتباط با سرور</div>';
    });
}

document.addEventListener('DOMContentLoaded', () => {
    setupOtpInput();
});
