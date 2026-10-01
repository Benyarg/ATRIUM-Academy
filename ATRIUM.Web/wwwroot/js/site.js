document.addEventListener('DOMContentLoaded', () => {
    const nav = document.querySelector('.site-nav');
    const navToggle = document.querySelector('[data-nav-toggle]');

    navToggle?.addEventListener('click', () => {
        const isOpen = nav?.classList.toggle('is-open') ?? false;
        navToggle.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
    });

    document.querySelectorAll('.site-menu a').forEach(link => {
        link.addEventListener('click', () => {
            nav?.classList.remove('is-open');
            navToggle?.setAttribute('aria-expanded', 'false');
        });
    });

    document.querySelector('[data-admin-toggle]')?.addEventListener('click', () => {
        document.querySelector('.admin-shell')?.classList.toggle('sidebar-open');
    });

    document.querySelectorAll('[data-confirm]').forEach(el => {
        el.addEventListener('click', e => {
            if (!confirm(el.getAttribute('data-confirm') || '¿Confirmar acción?')) e.preventDefault();
        });
    });
});
