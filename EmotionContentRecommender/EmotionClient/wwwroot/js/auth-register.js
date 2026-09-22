(() => {
    const form = document.getElementById("register-form");
    if (!form) return;

    const email = document.getElementById("Email");
    const mobile = document.getElementById("Mobile");
    const password = document.getElementById("Password");
    const confirmation = document.getElementById("ConfirmPassword");
    const contactRequirement = document.getElementById("contact-requirement");
    const passwordError = document.getElementById("password-match-error");
    const submit = document.getElementById("register-submit");
    const submitLabel = submit?.querySelector(".submit-label");

    const updateContactRequirement = (showError = false) => {
        const hasContact = Boolean(email.value.trim() || mobile.value.trim());
        email.setCustomValidity(hasContact ? "" : "Enter an email address or a mobile number.");
        contactRequirement.classList.toggle("is-complete", hasContact);
        contactRequirement.classList.toggle("is-error", !hasContact && showError);
        contactRequirement.lastChild.textContent = hasContact
            ? "Your contact method is ready."
            : "Enter at least one way to reach you: email or mobile.";
        return hasContact;
    };

    const updatePasswordMatch = (showError = false) => {
        const hasConfirmation = confirmation.value.length > 0;
        const matches = password.value === confirmation.value;
        confirmation.setCustomValidity(!hasConfirmation || matches ? "" : "The passwords do not match.");

        if (showError && hasConfirmation && !matches) {
            passwordError.textContent = "The passwords do not match.";
        } else if (passwordError.textContent === "The passwords do not match.") {
            passwordError.textContent = "";
        }

        return !hasConfirmation || matches;
    };

    document.querySelectorAll("[data-password-toggle]").forEach((toggle) => {
        const input = document.getElementById(toggle.dataset.passwordToggle);
        if (!input) return;

        toggle.addEventListener("click", () => {
            const isVisible = input.type === "text";
            input.type = isVisible ? "password" : "text";
            toggle.classList.toggle("is-visible", !isVisible);
            toggle.setAttribute("aria-pressed", String(!isVisible));
            toggle.setAttribute("aria-label", isVisible ? "Show password" : "Hide password");
            input.focus();
        });
    });

    [email, mobile].forEach((input) => {
        input.addEventListener("input", () => updateContactRequirement());
        input.addEventListener("blur", () => updateContactRequirement(true));
    });

    [password, confirmation].forEach((input) => {
        input.addEventListener("input", () => updatePasswordMatch());
        input.addEventListener("blur", () => updatePasswordMatch(true));
    });

    form.querySelectorAll("input, select").forEach((input) => {
        const control = input.closest(".field-control");
        if (!control) return;

        const updateErrorState = () => control.classList.toggle("has-error", !input.validity.valid);
        input.addEventListener("blur", updateErrorState);
        input.addEventListener("input", updateErrorState);
        input.addEventListener("change", updateErrorState);
    });

    form.addEventListener("submit", (event) => {
        const hasContact = updateContactRequirement(true);
        const passwordsMatch = updatePasswordMatch(true);

        if (!hasContact || !passwordsMatch || !form.checkValidity()) {
            event.preventDefault();
            form.reportValidity();
            return;
        }

        submit.disabled = true;
        submit.classList.add("is-loading");
        submitLabel.textContent = "Creating your account…";
    });

    updateContactRequirement();
    updatePasswordMatch();
})();
