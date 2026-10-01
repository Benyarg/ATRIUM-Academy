document.addEventListener("DOMContentLoaded", () => {
    const toggleButtons = document.querySelectorAll("[data-password-toggle]");

    toggleButtons.forEach(button => {
        button.addEventListener("click", () => {
            const inputId = button.dataset.passwordToggle;
            const input = inputId ? document.getElementById(inputId) : null;

            if (!(input instanceof HTMLInputElement)) {
                return;
            }

            const shouldShowPassword = input.type === "password";
            input.type = shouldShowPassword ? "text" : "password";
            button.textContent = shouldShowPassword ? "Ocultar" : "Ver";
            button.setAttribute(
                "aria-label",
                shouldShowPassword ? "Ocultar contraseña" : "Mostrar contraseña"
            );
        });
    });
});
