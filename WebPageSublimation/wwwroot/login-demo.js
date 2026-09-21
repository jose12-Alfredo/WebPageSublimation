(() => {
    document.addEventListener('click', event => {
        const account = event.target.closest('[data-demo-email]');
        if (!account) return;

        const form = account.closest('form');
        const email = form?.querySelector('#email');
        const password = form?.querySelector('#password');
        if (!email || !password) return;

        email.value = account.dataset.demoEmail;
        password.value = account.dataset.demoPassword;

        form.querySelectorAll('[data-demo-email]')
            .forEach(item => item.setAttribute('aria-pressed', String(item === account)));
        form.querySelector('button[type="submit"]')?.focus();
    });
})();
