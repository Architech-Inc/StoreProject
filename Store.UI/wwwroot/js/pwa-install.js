/**
 * ClexAn PWA Install Prompt & Standalone Manager
 */
(() => {
    'use strict';

    const DISMISS_KEY = 'clexan_pwa_install_dismissed';
    const COOLDOWN_DAYS = 7;

    let deferredPrompt = null;

    // Check if running in standalone mode
    const isStandalone = window.matchMedia('(display-mode: standalone)').matches ||
                         window.navigator.standalone === true;

    if (isStandalone) {
        console.info('ClexAn POS running in standalone native app shell.');
        return;
    }

    function isDismissedRecently() {
        try {
            const timestamp = localStorage.getItem(DISMISS_KEY);
            if (!timestamp) return false;
            const diff = Date.now() - parseInt(timestamp, 10);
            return diff < COOLDOWN_DAYS * 24 * 60 * 60 * 1000;
        } catch {
            return false;
        }
    }

    function createBanner() {
        if (document.getElementById('pwaInstallBanner')) return;

        const banner = document.createElement('div');
        banner.id = 'pwaInstallBanner';
        banner.className = 'pwa-install-banner';
        banner.setAttribute('role', 'banner');
        banner.innerHTML = `
            <div class="pwa-banner-icon">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="#ffffff" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect>
                    <line x1="8" y1="21" x2="16" y2="21"></line>
                    <line x1="12" y1="17" x2="12" y2="21"></line>
                </svg>
            </div>
            <div class="pwa-banner-content">
                <h5 class="pwa-banner-title">Install ClexAn POS</h5>
                <p class="pwa-banner-desc">Install for full-screen checkout & offline access</p>
            </div>
            <div class="pwa-banner-actions">
                <button type="button" class="btn-pwa-install" id="btnPwaInstall">Install</button>
                <button type="button" class="btn-pwa-dismiss" id="btnPwaDismiss" aria-label="Dismiss">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>
                </button>
            </div>
        `;

        document.body.appendChild(banner);

        document.getElementById('btnPwaInstall')?.addEventListener('click', async () => {
            if (!deferredPrompt) return;
            banner.classList.remove('visible');
            deferredPrompt.prompt();
            const { outcome } = await deferredPrompt.userChoice;
            console.info('PWA install prompt outcome:', outcome);
            deferredPrompt = null;
        });

        document.getElementById('btnPwaDismiss')?.addEventListener('click', () => {
            banner.classList.remove('visible');
            try {
                localStorage.setItem(DISMISS_KEY, Date.now().toString());
            } catch {}
        });

        // Delay showing banner slightly for smooth page entry
        setTimeout(() => {
            banner.classList.add('visible');
        }, 2000);
    }

    window.addEventListener('beforeinstallprompt', (e) => {
        e.preventDefault();
        deferredPrompt = e;

        if (!isDismissedRecently()) {
            createBanner();
        }

        // Show manual install link in user dropdown if present
        const manualInstallItem = document.getElementById('manualPwaInstallBtn');
        if (manualInstallItem) {
            manualInstallItem.style.display = 'flex';
        }
    });

    window.addEventListener('appinstalled', () => {
        deferredPrompt = null;
        const banner = document.getElementById('pwaInstallBanner');
        if (banner) banner.remove();
        console.info('ClexAn POS was installed successfully.');
    });

    // Public method for manual installation from context menu
    window.installClexAnApp = async () => {
        if (deferredPrompt) {
            deferredPrompt.prompt();
            const { outcome } = await deferredPrompt.userChoice;
            console.info('User choice on manual install:', outcome);
            deferredPrompt = null;
        } else {
            alert('To install ClexAn POS: Click the Install icon in your browser address bar or use Chrome menu -> Install ClexAn.');
        }
    };
})();
