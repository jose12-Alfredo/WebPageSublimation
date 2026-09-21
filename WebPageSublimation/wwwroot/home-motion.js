(() => {
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    let observer;

    function revealAll(elements) {
        elements.forEach(element => element.dataset.visible = 'true');
    }

    function initializeReveals() {
        const elements = document.querySelectorAll('[data-reveal]:not([data-motion-ready])');
        if (!elements.length) return;

        elements.forEach(element => element.dataset.motionReady = 'true');

        if (reducedMotion.matches || !('IntersectionObserver' in window)) {
            revealAll(elements);
            return;
        }

        observer ??= new IntersectionObserver(entries => {
            entries.forEach(entry => {
                if (!entry.isIntersecting) return;
                entry.target.dataset.visible = 'true';
                observer.unobserve(entry.target);
            });
        }, { threshold: 0.08, rootMargin: '0px 0px -8% 0px' });

        elements.forEach(element => observer.observe(element));
    }

    document.addEventListener('DOMContentLoaded', initializeReveals);
    document.addEventListener('enhancedload', initializeReveals);
    window.addEventListener('pageshow', initializeReveals);
})();
