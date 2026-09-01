// Global Portal JavaScript Helper
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

    // Auto-dismiss alerts
    document.querySelectorAll('.portal-alert.auto-dismiss').forEach(el => {
        setTimeout(() => {
            el.style.opacity = '0';
            el.style.transition = 'opacity 0.5s ease';
            setTimeout(() => el.remove(), 500);
        }, 5000);
    });
});

// Toast notification trigger
function showToast(message, type = 'success') {
    const toast = document.createElement('div');
    toast.className = `portal-alert ${type} auto-dismiss`;
    toast.style.position = 'fixed';
    toast.style.bottom = '24px';
    toast.style.right = '24px';
    toast.style.zIndex = '9999';
    toast.style.boxShadow = '0 10px 30px rgba(0,0,0,0.5)';
    toast.innerText = message;
    document.body.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transition = 'opacity 0.5s ease';
        setTimeout(() => toast.remove(), 500);
    }, 4000);
}

// Copy to clipboard helper
function copyToClipboard(text, btnElement) {
    navigator.clipboard.writeText(text).then(() => {
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
