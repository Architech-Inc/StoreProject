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
        let el = document.getElementById('toast-container');
        if (!el) {
            el = document.createElement('div');
            el.id = 'toast-container';
            el.className = 'toast-container';
            el.setAttribute('aria-live', 'polite');
            el.setAttribute('aria-atomic', 'false');
            document.body.appendChild(el);
        }
        return el;
    }

    /**
     * Publish a toast.
     * @param {string} channel  One of CHANNELS (default 'app') or level if shifting args.
     * @param {string} level    One of LEVELS (default 'info').
     * @param {string} message  Plain text (will be HTML-escaped).
     * @param {object} [opts]   duration (ms), actionLabel, actionHandler.
     */
    function publish(channel = 'app', level = 'info', message = '', opts = {}) {
        // If first argument is one of LEVELS, shift args for publish(level, message, opts) calling style
        if (LEVELS.includes(channel) && !CHANNELS.includes(channel)) {
            opts = typeof message === 'object' ? message : {};
            message = level;
            level = channel;
            channel = 'app';
        }

        const safeChannel = CHANNELS.includes(channel) ? channel : 'app';
        const safeLevel = LEVELS.includes(level) ? level : 'info';

        ensureContainer(safeChannel);
        if (typeof window.showToast === 'function') {
            window.showToast(safeLevel, message, { ...opts, channel: safeChannel });
        }
    }

    // Helper functions accepting (channel, msg, opts) OR (msg, opts) defaulting to 'app'
    function createHelper(level) {
        return (channelOrMsg, msgOrOpts, opts) => {
            if (CHANNELS.includes(channelOrMsg)) {
                publish(channelOrMsg, level, msgOrOpts, opts);
            } else {
                publish('app', level, channelOrMsg, typeof msgOrOpts === 'object' ? msgOrOpts : opts);
            }
        };
    }

    // Backwards-compat and canonical aliases
    window.ToastBus = {
        publish,
        success: createHelper('success'),
        info:    createHelper('info'),
        warning: createHelper('warning'),
        error:   createHelper('error')
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
