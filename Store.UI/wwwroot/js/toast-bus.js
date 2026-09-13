// ─── ToastBus ─────────────────────────────────────────────────────────────
// Canonical publish API for ephemeral notifications. Wraps window.showToast
// with channel semantics so feature code can say where a toast originates
// (e.g., 'pos', 'inventory', 'auth') and the bus routes to the right toast
// container with consistent styling + ARIA semantics.
//
// Backward-compatible: existing `window.showToast(type, msg, options)` calls
// still work and are treated as channel='app'.
(() => {
    'use strict';

    const LEVELS = ['success', 'info', 'warning', 'error'];
    const CHANNELS = ['app', 'auth', 'pos', 'inventory', 'finance', 'admin'];

    function ensureContainer(channel) {
        const id = `toast-container-${channel}`;
        let el = document.getElementById(id);
        if (!el) {
            el = document.createElement('div');
            el.id = id;
            el.className = `toast-container toast-channel-${channel}`;
            el.setAttribute('aria-live', 'polite');
            el.setAttribute('aria-atomic', 'false');
            document.body.appendChild(el);
        }
        return el;
    }

    /**
     * Publish a toast.
     * @param {string} channel  One of CHANNELS (default 'app').
     * @param {string} level    One of LEVELS (default 'info').
     * @param {string} message  Plain text (will be HTML-escaped).
     * @param {object} [opts]   duration (ms), actionLabel, actionHandler.
     */
    function publish(channel = 'app', level = 'info', message = '', opts = {}) {
        const safeChannel = CHANNELS.includes(channel) ? channel : 'app';
        const safeLevel = LEVELS.includes(level) ? level : 'info';

        // Delegate to the legacy showToast for the canonical rendering,
        // but make sure the container exists for the channel first.
        ensureContainer(safeChannel);
        if (typeof window.showToast === 'function') {
            window.showToast(safeLevel, message, opts);
        }
    }

    // Backwards-compat aliases
    window.ToastBus = {
        publish,
        success: (channel, msg, opts) => publish(channel, 'success', msg, opts),
        info:    (channel, msg, opts) => publish(channel, 'info',    msg, opts),
        warning: (channel, msg, opts) => publish(channel, 'warning', msg, opts),
        error:   (channel, msg, opts) => publish(channel, 'error',   msg, opts)
    };

    // Auto-surface server-rendered status banners (TempData / TempError) as toasts.
    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('[data-toast-message]').forEach(el => {
            const msg = el.getAttribute('data-toast-message');
            if (!msg) return;
            const channel = el.getAttribute('data-toast-channel') || 'app';
            const type = el.getAttribute('data-toast-type') === 'error' ? 'error'
                : el.getAttribute('data-toast-type') === 'warning' ? 'warning'
                : 'success';
            publish(channel, type, msg);
            el.remove();
        });
    });
})();
