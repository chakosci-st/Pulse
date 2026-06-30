(function () {
    const desktopQuery = window.matchMedia('(min-width: 992px)');
    const viewportPadding = 16;

    function syncStickyMenu(menu) {
        const anchor = menu.parentElement;
        if (!anchor) {
            return;
        }

        if (!desktopQuery.matches) {
            menu.style.left = '';
            menu.style.width = '';
            return;
        }

        const rect = anchor.getBoundingClientRect();
        const left = Math.max(Math.round(rect.left), viewportPadding);
        const availableWidth = Math.max(0, document.documentElement.clientWidth - left - viewportPadding);
        const width = Math.max(0, Math.min(Math.round(rect.width), availableWidth));

        menu.style.left = left + 'px';
        menu.style.width = width + 'px';
    }

    function syncAllStickyMenus() {
        document.querySelectorAll('.configurable--sticky').forEach(syncStickyMenu);
    }

    function scheduleStickyMenuSync() {
        window.requestAnimationFrame(syncAllStickyMenus);
        window.setTimeout(syncAllStickyMenus, 180);
    }

    function initStickyMenus() {
        scheduleStickyMenuSync();

        if ('ResizeObserver' in window) {
            const resizeObserver = new ResizeObserver(scheduleStickyMenuSync);
            document.querySelectorAll('.configurable--sticky').forEach(menu => {
                if (menu.parentElement) {
                    resizeObserver.observe(menu.parentElement);
                }
            });
        }

        if ('MutationObserver' in window && document.body) {
            const mutationObserver = new MutationObserver(scheduleStickyMenuSync);
            mutationObserver.observe(document.body, {
                attributes: true,
                attributeFilter: ['class']
            });
        }

        window.addEventListener('resize', scheduleStickyMenuSync);
        window.addEventListener('load', scheduleStickyMenuSync);

        if (desktopQuery.addEventListener) {
            desktopQuery.addEventListener('change', scheduleStickyMenuSync);
        } else if (desktopQuery.addListener) {
            desktopQuery.addListener(scheduleStickyMenuSync);
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initStickyMenus);
    } else {
        initStickyMenus();
    }
})();