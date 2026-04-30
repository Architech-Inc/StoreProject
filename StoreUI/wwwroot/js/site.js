(() => {
    // Modal open/close via data attributes
    const openTiles = document.querySelectorAll('[data-open-modal]');
    const closeButtons = document.querySelectorAll('[data-close-modal]');
    const allModals = document.querySelectorAll('.modalView');

    const closeAll = () => {
        allModals.forEach(m => m.classList.remove('show'));
    };

    openTiles.forEach(tile => {
        tile.addEventListener('click', () => {
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
})();
