(() => {
    const card = document.getElementById("otp-card");
    if (!card) return;

    const mobile = card.dataset.mobile;
    const sendUrl = card.dataset.sendUrl;
    const verifyUrl = card.dataset.verifyUrl;
    const loginUrl = card.dataset.loginUrl;
    const sendState = document.getElementById("otp-send-state");
    const sendButton = document.getElementById("send-otp-button");
    const codeSection = document.getElementById("otp-code-section");
    const inputRow = document.getElementById("otp-input-row");
    const inputs = Array.from(inputRow.querySelectorAll("input"));
    const verifyButton = document.getElementById("verify-otp-button");
    const resendButton = document.getElementById("resend-otp-button");
    const expiry = document.getElementById("otp-expiry");
    const codeError = document.getElementById("otp-code-error");
    const message = document.getElementById("otp-message");
    let expiryTimer;
    let resendTimer;
    let isExpired = false;

    const setLoading = (button, loading, label) => {
        button.disabled = loading;
        button.classList.toggle("is-loading", loading);
        button.querySelector(".submit-label").textContent = label;
    };

    const showMessage = (text, type) => {
        message.textContent = text;
        message.className = `otp-message is-${type}`;
        message.hidden = false;
    };

    const clearMessage = () => {
        message.hidden = true;
        message.textContent = "";
    };

    const getCode = () => inputs.map((input) => input.value).join("");

    const updateCodeState = () => {
        const complete = getCode().length === 6;
        verifyButton.disabled = !complete || isExpired;
        if (complete) {
            inputRow.classList.remove("is-error");
            codeError.textContent = "";
        }
    };

    const formatTimer = (seconds) => {
        const minutes = Math.floor(seconds / 60);
        const remainder = seconds % 60;
        return `${String(minutes).padStart(2, "0")}:${String(remainder).padStart(2, "0")}`;
    };

    const startTimers = () => {
        clearInterval(expiryTimer);
        clearInterval(resendTimer);
        isExpired = false;
        expiry.classList.remove("is-expired");

        let expirySeconds = 120;
        expiry.textContent = formatTimer(expirySeconds);
        expiryTimer = window.setInterval(() => {
            expirySeconds -= 1;
            expiry.textContent = formatTimer(Math.max(expirySeconds, 0));
            if (expirySeconds <= 0) {
                clearInterval(expiryTimer);
                isExpired = true;
                expiry.textContent = "Expired";
                expiry.classList.add("is-expired");
                verifyButton.disabled = true;
                codeError.textContent = "This code has expired. Request a new one.";
            }
        }, 1000);

        let resendSeconds = 60;
        resendButton.disabled = true;
        resendButton.textContent = `Resend in ${formatTimer(resendSeconds)}`;
        resendTimer = window.setInterval(() => {
            resendSeconds -= 1;
            resendButton.textContent = `Resend in ${formatTimer(Math.max(resendSeconds, 0))}`;
            if (resendSeconds <= 0) {
                clearInterval(resendTimer);
                resendButton.disabled = false;
                resendButton.textContent = "Resend code";
            }
        }, 1000);
    };

    const sendCode = async () => {
        const isResend = !codeSection.hidden;
        clearMessage();
        if (isResend) {
            resendButton.disabled = true;
            resendButton.textContent = "Sending…";
        } else {
            setLoading(sendButton, true, "Sending code…");
        }

        try {
            const response = await fetch(sendUrl, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ mobile })
            });
            const result = await response.json();
            if (!response.ok || !result.isSuccess) {
                throw new Error("send_failed");
            }

            sendState.hidden = true;
            codeSection.hidden = false;
            inputs.forEach((input) => {
                input.value = "";
                input.disabled = false;
            });
            showMessage("Verification code sent. It will expire in two minutes.", "success");
            startTimers();
            updateCodeState();
            inputs[0].focus();
        } catch {
            showMessage("We could not send the code. Please wait a moment and try again.", "error");
            if (isResend) {
                resendButton.disabled = false;
                resendButton.textContent = "Resend code";
            } else {
                sendButton.disabled = false;
            }
        } finally {
            if (!isResend) {
                sendButton.classList.remove("is-loading");
                sendButton.querySelector(".submit-label").textContent = "Send verification code";
            }
        }
    };

    const verifyCode = async () => {
        const code = getCode();
        if (code.length !== 6 || isExpired) {
            inputRow.classList.add("is-error");
            codeError.textContent = isExpired
                ? "This code has expired. Request a new one."
                : "Enter all six digits.";
            return;
        }

        clearMessage();
        setLoading(verifyButton, true, "Verifying…");

        try {
            const response = await fetch(verifyUrl, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ mobile, code })
            });
            const result = await response.json();
            if (!response.ok || !result.isSuccess) {
                throw new Error("verify_failed");
            }

            clearInterval(expiryTimer);
            clearInterval(resendTimer);
            inputs.forEach((input) => input.disabled = true);
            showMessage("Mobile number verified. Taking you to sign in…", "success");
            setLoading(verifyButton, true, "Verified");
            window.setTimeout(() => window.location.assign(loginUrl), 1200);
        } catch {
            inputRow.classList.add("is-error");
            codeError.textContent = "That code is invalid or has expired. Check it and try again.";
            setLoading(verifyButton, false, "Verify and continue");
            inputs[0].focus();
        }
    };

    inputs.forEach((input, index) => {
        input.addEventListener("input", () => {
            input.value = input.value.replace(/\D/g, "").slice(-1);
            if (input.value && index < inputs.length - 1) inputs[index + 1].focus();
            updateCodeState();
        });

        input.addEventListener("keydown", (event) => {
            if (event.key === "Backspace" && !input.value && index > 0) inputs[index - 1].focus();
            if (event.key === "ArrowLeft" && index > 0) inputs[index - 1].focus();
            if (event.key === "ArrowRight" && index < inputs.length - 1) inputs[index + 1].focus();
            if (event.key === "Enter" && !verifyButton.disabled) verifyCode();
        });

        input.addEventListener("paste", (event) => {
            event.preventDefault();
            const digits = event.clipboardData.getData("text").replace(/\D/g, "").slice(0, 6);
            digits.split("").forEach((digit, digitIndex) => {
                inputs[digitIndex].value = digit;
            });
            inputs[Math.min(digits.length, inputs.length) - 1]?.focus();
            updateCodeState();
        });
    });

    sendButton.addEventListener("click", sendCode);
    verifyButton.addEventListener("click", verifyCode);
    resendButton.addEventListener("click", sendCode);
})();
