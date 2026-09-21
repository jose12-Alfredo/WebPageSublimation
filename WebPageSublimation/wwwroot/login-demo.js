(() => {
    function markSelected(form, selected) {
        form.querySelectorAll('[data-demo-email]')
            .forEach(item => item.setAttribute('aria-pressed', String(item === selected)));
    }

    document.addEventListener('click', event => {
        const account = event.target.closest('[data-demo-email]');
        if (!account) return;

        const form = account.closest('form');
        const email = form?.querySelector('#email');
        const password = form?.querySelector('#password');
        if (!email || !password) return;

        email.value = account.dataset.demoEmail;
        password.value = account.dataset.demoPassword;
        markSelected(form, account);
        form.querySelector('button[type="submit"]')?.focus();
    });

    // Si el usuario edita los campos a mano, la tarjeta deja de representar lo escrito.
    document.addEventListener('input', event => {
        const form = event.target.closest('form');
        if (!form?.querySelector('[data-demo-email]')) return;
        if (event.target.id !== 'email' && event.target.id !== 'password') return;
        markSelected(form, null);
    });
})();
