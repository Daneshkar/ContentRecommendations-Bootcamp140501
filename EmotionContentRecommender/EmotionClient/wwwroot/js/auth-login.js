(() => {
    const form = document.getElementById('login-form');
    const password = document.getElementById('Password');
    const toggle = document.querySelector('.password-toggle');
    const submit = document.getElementById('login-submit');
    const submitLabel = submit?.querySelector('.submit-label');

    if (password && toggle) {
        toggle.addEventListener('click', () => {
            const isVisible = password.type === 'text';
            password.type = isVisible ? 'password' : 'text';
            toggle.classList.toggle('is-visible', !isVisible);
            toggle.setAttribute('aria-pressed', String(!isVisible));
            toggle.setAttribute('aria-label', isVisible ? 'Show password' : 'Hide password');
            password.focus();
        });
    }

    if (form && submit && submitLabel) {
        form.addEventListener('submit', () => {
            if (!form.checkValidity()) return;

            submit.disabled = true;
            submit.classList.add('is-loading');
            submitLabel.textContent = 'Signing in…';
        });
    }
})();
