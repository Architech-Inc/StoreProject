(() => {
    // Modal open/close via data attributes
    const openTiles = document.querySelectorAll('[data-open-modal]');
    const closeButtons = document.querySelectorAll('[data-close-modal]');
    const allModals = document.querySelectorAll('.modalView');

    const closeAll = () => {
        allModals.forEach(m => m.classList.remove('show'));
    };

    openTiles.forEach(tile => {
        tile.addEventListener('click', e => {
            e.preventDefault();
            const id = tile.getAttribute('data-open-modal');
            closeAll();
            if (id) {
                const target = document.getElementById(id);
                if (target) target.classList.add('show');
            }
        });
    });

    closeButtons.forEach(btn => {
        btn.addEventListener('click', closeAll);
    });

    // Click outside modal content closes it
    allModals.forEach(modal => {
        modal.addEventListener('click', e => {
            if (e.target === modal) closeAll();
        });
    });

    // Stack panel toggle via headerMenu button
    const menuBtn = document.querySelector('.headerMenu');
    const panel = document.getElementById('stackPanel');
    const exitPanelBtn = document.getElementById('exitPanel');

    if (menuBtn && panel) {
        menuBtn.addEventListener('click', () => {
            panel.style.display = panel.style.display === 'none' ? 'flex' : 'none';
        });
    }

    if (exitPanelBtn && panel) {
        exitPanelBtn.style.display = 'block';
        exitPanelBtn.addEventListener('click', () => {
            panel.style.display = 'none';
        });
    }

    // Logout nav item
    const logoutLi = document.getElementById('clickId_log');
    if (logoutLi) {
        logoutLi.addEventListener('click', () => {
            window.location.href = '/Login';
        });
    }

    // --- Toasts ---
    window.showToast = (type, message) => {
        const container = document.getElementById('toast-container');
        if (!container) return;

        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        
        const icon = type === 'success' ? '✓' : '!';
        
        toast.innerHTML = `
            <div class="toast-icon">${icon}</div>
            <div class="toast-message">${message}</div>
            <button class="toast-close" aria-label="Close">&times;</button>
        `;

        container.appendChild(toast);

        requestAnimationFrame(() => {
            toast.classList.add('show');
        });

        const dismiss = () => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        };

        toast.querySelector('.toast-close').addEventListener('click', dismiss);
        setTimeout(dismiss, 5000);
    };

    // --- Blades ---
    window.openBlade = (id) => {
        const blade = document.getElementById(id);
        if (!blade) return;
        const overlayId = blade.getAttribute('data-overlay-id');
        if (overlayId) {
            const overlay = document.getElementById(overlayId);
            if (overlay) overlay.classList.add('open');
        }
        blade.classList.add('open');
    };

    window.closeBlade = (id) => {
        const blade = document.getElementById(id);
        if (!blade) return;
        const overlayId = blade.getAttribute('data-overlay-id');
        if (overlayId) {
            const overlay = document.getElementById(overlayId);
            if (overlay) overlay.classList.remove('open');
        }
        blade.classList.remove('open');
    };

    document.querySelectorAll('[data-close-blade]').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const id = btn.getAttribute('data-close-blade');
            window.closeBlade(id);
        });
    });

    document.querySelectorAll('.blade-overlay').forEach(overlay => {
        overlay.addEventListener('click', () => {
            const bladeId = overlay.getAttribute('data-blade-id');
            if (bladeId) window.closeBlade(bladeId);
        });
    });

    // App Shell Mobile Menu Toggle
    const menuToggle = document.getElementById('menuToggle');
    const appShell = document.getElementById('appShell');
    
    if (menuToggle && appShell) {
        menuToggle.addEventListener('click', () => {
            appShell.classList.toggle('sidebar-collapsed');
            const isOpen = appShell.classList.contains('sidebar-collapsed');
            menuToggle.setAttribute('aria-expanded', isOpen);
        });

        // Close sidebar when clicking outside (on the overlay)
        appShell.addEventListener('click', (e) => {
            if (e.target === appShell && appShell.classList.contains('sidebar-collapsed')) {
                appShell.classList.remove('sidebar-collapsed');
                menuToggle.setAttribute('aria-expanded', 'false');
            }
        });
    }

})();
