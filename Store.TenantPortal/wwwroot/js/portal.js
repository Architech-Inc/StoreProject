// Global ClexAn Portal Helper & Custom UI Dialog System (Alert, Confirm, Prompt, Toast)

const ClexAn = {
    // 1. Custom Alert Dialog
    alert(message, title = 'Notice', type = 'info') {
        return new Promise((resolve) => {
            const modal = createModalElement({
                title,
                message,
                type,
                showInput: false,
                confirmText: 'Got It',
                cancelText: null,
                isDanger: type === 'error' || type === 'danger'
            });

            modal.btnConfirm.addEventListener('click', () => {
                destroyModal(modal);
                resolve();
            });

            document.body.appendChild(modal.backdrop);
            requestAnimationFrame(() => modal.backdrop.classList.add('show'));
            modal.btnConfirm.focus();
        });
    },

    // 2. Custom Confirm Dialog
    confirm(message, title = 'Confirm Action', options = {}) {
        const {
            confirmText = 'Confirm',
            cancelText = 'Cancel',
            type = 'warning',
            isDanger = false
        } = options;

        return new Promise((resolve) => {
            const modal = createModalElement({
                title,
                message,
                type: isDanger ? 'danger' : type,
                showInput: false,
                confirmText,
                cancelText,
                isDanger
            });

            const cleanup = (result) => {
                destroyModal(modal);
                resolve(result);
            };

            modal.btnConfirm.addEventListener('click', () => cleanup(true));
            modal.btnCancel.addEventListener('click', () => cleanup(false));
            modal.backdrop.addEventListener('click', (e) => {
                if (e.target === modal.backdrop) cleanup(false);
            });

            document.body.appendChild(modal.backdrop);
            requestAnimationFrame(() => modal.backdrop.classList.add('show'));
            modal.btnConfirm.focus();
        });
    },

    // 3. Custom Prompt Dialog
    prompt(message, defaultValue = '', title = 'Input Required', options = {}) {
        const {
            placeholder = '',
            confirmText = 'Submit',
            cancelText = 'Cancel',
            type = 'info'
        } = options;

        return new Promise((resolve) => {
            const modal = createModalElement({
                title,
                message,
                type,
                showInput: true,
                defaultValue,
                placeholder,
                confirmText,
                cancelText,
                isDanger: false
            });

            const cleanup = (result) => {
                destroyModal(modal);
                resolve(result);
            };

            modal.btnConfirm.addEventListener('click', () => cleanup(modal.input.value));
            modal.btnCancel.addEventListener('click', () => cleanup(null));
            modal.input.addEventListener('keydown', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    cleanup(modal.input.value);
                } else if (e.key === 'Escape') {
                    e.preventDefault();
                    cleanup(null);
                }
            });

            document.body.appendChild(modal.backdrop);
            requestAnimationFrame(() => modal.backdrop.classList.add('show'));
            modal.input.focus();
            modal.input.select();
        });
    },

    // 4. Custom Toast Notification
    toast(message, type = 'success', duration = 3500) {
        let container = document.getElementById('clexanToastContainer');
        if (!container) {
            container = document.createElement('div');
            container.id = 'clexanToastContainer';
            container.className = 'clexan-toast-container';
            document.body.appendChild(container);
        }

        const toast = document.createElement('div');
        toast.className = `clexan-toast ${type}`;

        const iconSvg = getIconSvg(type);
        toast.innerHTML = `
            <div style="flex-shrink: 0;">${iconSvg}</div>
            <div style="flex: 1; line-height: 1.4;">${escapeHtml(message)}</div>
        `;

        container.appendChild(toast);
        requestAnimationFrame(() => toast.classList.add('show'));

        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 350);
        }, duration);
    }
};

// Helper: Modal Element Builder
function createModalElement(config) {
    const backdrop = document.createElement('div');
    backdrop.className = 'clexan-modal-backdrop';

    const card = document.createElement('div');
    card.className = 'clexan-modal-card';

    const iconSvg = getIconSvg(config.type);

    card.innerHTML = `
        <div class="clexan-modal-header">
            <div class="clexan-modal-icon-wrap ${config.type}">
                ${iconSvg}
            </div>
            <h3 class="clexan-modal-title">${escapeHtml(config.title)}</h3>
        </div>
        <div class="clexan-modal-body">
            <div>${escapeHtml(config.message)}</div>
            ${config.showInput ? `
                <div class="clexan-modal-input-wrap">
                    <input type="text" class="clexan-modal-input" placeholder="${escapeHtml(config.placeholder || '')}" value="${escapeHtml(config.defaultValue || '')}" />
                </div>
            ` : ''}
        </div>
        <div class="clexan-modal-footer">
            ${config.cancelText ? `
                <button type="button" class="btn-glass modal-btn-cancel" style="padding: 9px 20px; font-size: 13.5px;">
                    ${escapeHtml(config.cancelText)}
                </button>
            ` : ''}
            <button type="button" class="${config.isDanger ? 'btn-danger-glass' : 'btn-primary-glow'} modal-btn-confirm" style="padding: 9px 22px; font-size: 13.5px;">
                ${escapeHtml(config.confirmText)}
            </button>
        </div>
    `;

    backdrop.appendChild(card);

    return {
        backdrop,
        card,
        input: card.querySelector('.clexan-modal-input'),
        btnConfirm: card.querySelector('.modal-btn-confirm'),
        btnCancel: card.querySelector('.modal-btn-cancel')
    };
}

function destroyModal(modal) {
    modal.backdrop.classList.remove('show');
    setTimeout(() => modal.backdrop.remove(), 250);
}

function getIconSvg(type) {
    switch (type) {
        case 'success':
            return '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#4ade80" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"></polyline></svg>';
        case 'warning':
            return '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#fbbf24" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path><line x1="12" y1="9" x2="12" y2="13"></line><line x1="12" y1="17" x2="12.01" y2="17"></line></svg>';
        case 'danger':
        case 'error':
            return '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#f87171" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="15" y1="9" x2="9" y2="15"></line><line x1="9" y1="9" x2="15" y2="15"></line></svg>';
        default:
            return '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#38bdf8" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="16" x2="12" y2="12"></line><line x1="12" y1="8" x2="12.01" y2="8"></line></svg>';
    }
}

function escapeHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

// Expose globally on window
window.ClexAn = ClexAn;
window.showToast = (msg, type) => ClexAn.toast(msg, type);

// Seamless modern overrides for native browser dialogs
window.alert = (msg) => ClexAn.alert(msg);
window.confirm = (msg) => ClexAn.confirm(msg);
window.prompt = (msg, def) => ClexAn.prompt(msg, def);

// Standard DOM load behavior
document.addEventListener('DOMContentLoaded', () => {
    // Navbar scroll effect
    const header = document.querySelector('.portal-header');
    if (header) {
        window.addEventListener('scroll', () => {
            if (window.scrollY > 20) {
                header.style.background = 'rgba(5, 9, 6, 0.85)';
                header.style.boxShadow = '0 8px 32px rgba(0, 0, 0, 0.3)';
            } else {
                header.style.background = 'rgba(5, 9, 6, 0.6)';
                header.style.boxShadow = 'none';
            }
        });
    }
});

// Copy to clipboard helper with custom toast feedback
function copyToClipboard(text, btnElement) {
    navigator.clipboard.writeText(text).then(() => {
        ClexAn.toast('Copied to clipboard!', 'success');
        if (btnElement) {
            const originalText = btnElement.innerText;
            btnElement.innerText = 'Copied!';
            btnElement.style.borderColor = 'var(--p-green)';
            setTimeout(() => {
                btnElement.innerText = originalText;
                btnElement.style.borderColor = '';
            }, 2000);
        }
    });
}
