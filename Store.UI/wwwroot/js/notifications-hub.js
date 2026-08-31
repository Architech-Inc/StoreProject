/**
 * ClexAn Real-Time Activity Center & SignalR Hub Client
 */
(() => {
    'use strict';

    const STORAGE_KEY = 'clexan_notifications_history';
    const MAX_STORED = 50;

    let connection = null;
    let notifications = [];
    let activeFilter = 'all';

    // DOM Elements
    const notifBtn = document.getElementById('topbarNotifBtn');
    const notifBadge = document.getElementById('notifBadgeCount');
    const drawerOverlay = document.getElementById('notifDrawerOverlay');
    const drawerClose = document.getElementById('notifDrawerClose');
    const notifList = document.getElementById('notifList');
    const emptyState = document.getElementById('notifEmptyState');
    const markAllReadBtn = document.getElementById('notifMarkAllReadBtn');
    const clearHistoryBtn = document.getElementById('notifClearHistoryBtn');
    const soundToggle = document.getElementById('notifSoundToggle');
    const statusIndicator = document.getElementById('notifConnectionStatus');
    const tabs = document.querySelectorAll('.notif-tab');

    // Audio synthesizer for notification chime
    function playChime() {
        if (!soundToggle || !soundToggle.checked) return;
        try {
            const ctx = new (window.AudioContext || window.webkitAudioContext)();
            const now = ctx.currentTime;
            
            const osc1 = ctx.createOscillator();
            const osc2 = ctx.createOscillator();
            const gain = ctx.createGain();

            osc1.type = 'sine';
            osc1.frequency.setValueAtTime(587.33, now); // D5
            osc1.frequency.exponentialRampToValueAtTime(880, now + 0.15); // A5

            osc2.type = 'triangle';
            osc2.frequency.setValueAtTime(440, now);
            osc2.frequency.exponentialRampToValueAtTime(659.25, now + 0.15); // E5

            gain.gain.setValueAtTime(0.15, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.5);

            osc1.connect(gain);
            osc2.connect(gain);
            gain.connect(ctx.destination);

            osc1.start(now);
            osc2.start(now);
            osc1.stop(now + 0.5);
            osc2.stop(now + 0.5);
        } catch {
            // Audio context not allowed without prior user interaction
        }
    }

    // Load from LocalStorage
    function loadNotifications() {
        try {
            const raw = localStorage.getItem(STORAGE_KEY);
            notifications = raw ? JSON.parse(raw) : [];
        } catch {
            notifications = [];
        }
    }

    function saveNotifications() {
        try {
            if (notifications.length > MAX_STORED) {
                notifications = notifications.slice(0, MAX_STORED);
            }
            localStorage.setItem(STORAGE_KEY, JSON.stringify(notifications));
        } catch (e) {
            console.warn('Failed to save notifications to localStorage', e);
        }
    }

    function updateBadge() {
        if (!notifBadge) return;
        const unreadCount = notifications.filter(n => !n.read).length;
        if (unreadCount > 0) {
            notifBadge.textContent = unreadCount > 99 ? '99+' : unreadCount;
            notifBadge.style.display = 'flex';
        } else {
            notifBadge.style.display = 'none';
        }
    }

    function renderNotifications() {
        if (!notifList || !emptyState) return;

        const filtered = activeFilter === 'all'
            ? notifications
            : notifications.filter(n => n.category === activeFilter || String(n.category) === activeFilter);

        if (filtered.length === 0) {
            notifList.innerHTML = '';
            emptyState.style.display = 'flex';
            return;
        }

        emptyState.style.display = 'none';
        notifList.innerHTML = filtered.map(n => {
            const timeAgo = formatTimeAgo(new Date(n.dateCreated));
            const unreadCls = n.read ? '' : 'unread';
            const actionHtml = n.targetUrl
                ? `<a href="${n.targetUrl}" class="notif-card-action">${n.actionLabel || 'View Details'} &rarr;</a>`
                : '';

            return `
                <div class="notif-card severity-${n.severity || 'Info'} ${unreadCls}" data-id="${n.id}">
                    <div class="notif-card-header">
                        <h5 class="notif-card-title">${escapeHtml(n.title)}</h5>
                        <span class="notif-card-time">${timeAgo}</span>
                    </div>
                    <p class="notif-card-msg">${escapeHtml(n.message)}</p>
                    ${actionHtml}
                </div>
            `;
        }).join('');

        // Attach click to mark as read
        notifList.querySelectorAll('.notif-card').forEach(card => {
            card.addEventListener('click', () => {
                const id = card.getAttribute('data-id');
                markAsRead(id);
            });
        });
    }

    function markAsRead(id) {
        const item = notifications.find(n => n.id === id);
        if (item && !item.read) {
            item.read = true;
            saveNotifications();
            updateBadge();
            renderNotifications();
        }
    }

    function markAllAsRead() {
        const cards = notifList ? notifList.querySelectorAll('.notif-card.unread') : [];
        cards.forEach(c => c.classList.remove('unread'));

        notifications.forEach(n => n.read = true);
        saveNotifications();
        updateBadge();
    }

    function clearHistory() {
        const cards = notifList ? notifList.querySelectorAll('.notif-card') : [];
        if (cards.length > 0) {
            cards.forEach(c => c.classList.add('fade-out'));
            setTimeout(() => {
                notifications = [];
                saveNotifications();
                updateBadge();
                renderNotifications();
            }, 250);
        } else {
            notifications = [];
            saveNotifications();
            updateBadge();
            renderNotifications();
        }
    }

    function addNotification(notif) {
        const item = {
            id: notif.id || (Date.now().toString(36) + Math.random().toString(36).substring(2)),
            title: notif.title || 'System Notification',
            message: notif.message || '',
            category: notif.category || 'General',
            severity: notif.severity || 'Info',
            targetUrl: notif.targetUrl || null,
            actionLabel: notif.actionLabel || null,
            dateCreated: notif.dateCreated || new Date().toISOString(),
            read: false
        };

        notifications.unshift(item);
        saveNotifications();
        updateBadge();
        renderNotifications();
        playChime();

        // Trigger in-app toast if toast helper exists
        if (window.showToast) {
            window.showToast(item.message, item.severity === 'Danger' ? 'error' : (item.severity === 'Success' ? 'success' : 'info'));
        }
    }

    function formatTimeAgo(date) {
        const seconds = Math.floor((new Date() - date) / 1000);
        if (seconds < 60) return 'Just now';
        const minutes = Math.floor(seconds / 60);
        if (minutes < 60) return `${minutes}m ago`;
        const hours = Math.floor(minutes / 60);
        if (hours < 24) return `${hours}h ago`;
        const days = Math.floor(hours / 24);
        return `${days}d ago`;
    }

    function escapeHtml(str) {
        if (!str) return '';
        return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    // UI Event Listeners
    if (notifBtn) {
        notifBtn.addEventListener('click', () => {
            drawerOverlay?.classList.add('open');
            renderNotifications();
        });
    }

    drawerClose?.addEventListener('click', () => drawerOverlay?.classList.remove('open'));
    drawerOverlay?.addEventListener('click', (e) => {
        if (e.target === drawerOverlay) drawerOverlay.classList.remove('open');
    });

    markAllReadBtn?.addEventListener('click', markAllAsRead);
    clearHistoryBtn?.addEventListener('click', clearHistory);

    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            tabs.forEach(t => t.classList.remove('active'));
            tab.classList.add('active');
            activeFilter = tab.getAttribute('data-filter') || 'all';
            renderNotifications();
        });
    });

    // Initialize SignalR Connection
    function initSignalR() {
        const token = sessionStorage.getItem('access_token') || localStorage.getItem('access_token');
        if (typeof signalR === 'undefined') {
            console.info('SignalR library not loaded yet; notifications initialized in local mode.');
            return;
        }

        try {
            connection = new signalR.HubConnectionBuilder()
                .withUrl('/hubs/notifications', {
                    accessTokenFactory: () => token || ''
                })
                .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
                .configureLogging(signalR.LogLevel.Warning)
                .build();

            connection.on('ReceiveNotification', (notif) => {
                addNotification(notif);
            });

            connection.on('ReceiveDiscountOverrideUpdate', (dto) => {
                addNotification({
                    title: `Discount Override ${dto.status}`,
                    message: `Your request for ${dto.requestedDiscount} XAF discount was ${dto.status.toLowerCase()}.`,
                    category: 'DiscountApproval',
                    severity: dto.status === 'Approved' ? 'Success' : 'Warning',
                    targetUrl: '/DiscountOverrides'
                });

                // Dispatch global event for POS
                window.dispatchEvent(new CustomEvent('discount-override-approved', { detail: dto }));
            });

            connection.on('ReceiveLowStockAlert', (dto) => {
                addNotification({
                    title: 'Low Stock Alert',
                    message: `${dto.itemName} is low (${dto.currentStock} units left, reorder at ${dto.reorderLevel}).`,
                    category: 'LowStock',
                    severity: 'Warning',
                    targetUrl: `/Catalog?search=${encodeURIComponent(dto.itemName)}`,
                    actionLabel: 'Restock'
                });
            });

            connection.onreconnecting(() => {
                if (statusIndicator) {
                    statusIndicator.classList.add('offline');
                    statusIndicator.innerHTML = '<span class="status-dot"></span> Reconnecting...';
                }
            });

            connection.onreconnected(() => {
                if (statusIndicator) {
                    statusIndicator.classList.remove('offline');
                    statusIndicator.innerHTML = '<span class="status-dot"></span> Live';
                }
            });

            connection.onclose(() => {
                if (statusIndicator) {
                    statusIndicator.classList.add('offline');
                    statusIndicator.innerHTML = '<span class="status-dot"></span> Offline';
                }
            });

            connection.start()
                .then(() => {
                    if (statusIndicator) {
                        statusIndicator.classList.remove('offline');
                        statusIndicator.innerHTML = '<span class="status-dot"></span> Live';
                    }
                })
                .catch(() => {
                    if (statusIndicator) {
                        statusIndicator.classList.add('offline');
                        statusIndicator.innerHTML = '<span class="status-dot"></span> Local Mode';
                    }
                });
        } catch (err) {
            console.warn('SignalR initialization skipped:', err);
        }
    }

    // On Load
    loadNotifications();
    updateBadge();
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initSignalR);
    } else {
        initSignalR();
    }

    // Expose for testing/manual triggering
    window.ClexAnNotifications = {
        push: addNotification,
        clear: clearHistory,
        markAllRead: markAllAsRead
    };
})();
