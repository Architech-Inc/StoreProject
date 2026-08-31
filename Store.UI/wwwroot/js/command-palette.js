/**
 * Global Omnisearch & Command Palette (Ctrl + K)
 * Fast-action launcher for POS, Catalog, Invoices, Customers, Suppliers, and Scanner Resolution.
 */
(() => {
    let overlay = null;
    let input = null;
    let spinner = null;
    let dynamicGroup = null;
    let dynamicList = null;
    let navGroup = null;
    let searchDebounceTimer = null;
    let selectedIndex = 0;

    function initCommandPalette() {
        overlay = document.getElementById('commandPaletteOverlay');
        if (!overlay) return;

        input = document.getElementById('cmdPaletteInput');
        spinner = document.getElementById('cmdSpinner');
        dynamicGroup = document.getElementById('cmdDynamicResults');
        dynamicList = document.getElementById('cmdDynamicList');
        navGroup = document.getElementById('cmdNavShortcuts');

        // Topbar trigger
        const trigger = document.getElementById('topbarSearchTrigger');
        if (trigger) {
            trigger.addEventListener('click', openPalette);
        }

        // Global Keydown Listener
        window.addEventListener('keydown', (e) => {
            if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
                e.preventDefault();
                togglePalette();
            } else if (e.key === 'Escape' && isVisible()) {
                e.preventDefault();
                closePalette();
            }
        });

        // Overlay click outside box
        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) {
                closePalette();
            }
        });

        // Input events
        if (input) {
            input.addEventListener('input', handleInputChange);
            input.addEventListener('keydown', handleKeyNavigation);
        }

        // Item click delegation
        overlay.addEventListener('click', (e) => {
            const item = e.target.closest('.cmd-item');
            if (item) {
                const url = item.getAttribute('data-url');
                if (url) {
                    window.location.href = url;
                }
            }
        });
    }

    function isVisible() {
        return overlay && !overlay.hidden && overlay.style.display !== 'none';
    }

    function openPalette() {
        if (!overlay) return;
        overlay.hidden = false;
        overlay.style.display = 'flex';
        if (input) {
            input.value = '';
            input.focus();
        }
        if (dynamicGroup) dynamicGroup.style.display = 'none';
        if (navGroup) navGroup.style.display = 'block';
        updateSelection(0);
    }

    function closePalette() {
        if (!overlay) return;
        overlay.hidden = true;
        overlay.style.display = 'none';
    }

    function togglePalette() {
        if (isVisible()) {
            closePalette();
        } else {
            openPalette();
        }
    }

    function handleInputChange() {
        const query = input.value.trim();
        clearTimeout(searchDebounceTimer);

        if (!query) {
            if (dynamicGroup) dynamicGroup.style.display = 'none';
            if (navGroup) navGroup.style.display = 'block';
            if (spinner) spinner.style.display = 'none';
            updateSelection(0);
            return;
        }

        // Check local nav matches
        filterNavShortcuts(query);

        // Debounce API search
        if (spinner) spinner.style.display = 'block';
        searchDebounceTimer = setTimeout(() => {
            performSearch(query);
        }, 220);
    }

    function filterNavShortcuts(query) {
        const q = query.toLowerCase();
        const items = navGroup.querySelectorAll('.cmd-item');
        let hasMatches = false;

        items.forEach(item => {
            const title = item.querySelector('.cmd-item-title')?.textContent.toLowerCase() || '';
            const desc = item.querySelector('.cmd-item-desc')?.textContent.toLowerCase() || '';
            if (title.includes(q) || desc.includes(q)) {
                item.style.display = 'flex';
                hasMatches = true;
            } else {
                item.style.display = 'none';
            }
        });

        navGroup.style.display = hasMatches ? 'block' : 'none';
    }

    async function performSearch(query) {
        try {
            const res = await fetch(`/api/scanner/resolve?code=${encodeURIComponent(query)}`);
            if (!res.ok) throw new Error('Search failed');

            const payload = await res.json();
            const hit = payload?.data;

            if (hit && hit.entityType) {
                renderSearchHit(hit);
            } else {
                renderNoHit();
            }
        } catch (err) {
            renderNoHit();
        } finally {
            if (spinner) spinner.style.display = 'none';
            updateSelection(0);
        }
    }

    function renderSearchHit(hit) {
        if (!dynamicGroup || !dynamicList) return;
        dynamicGroup.style.display = 'block';
        dynamicList.innerHTML = '';

        const iconMap = {
            'Item': '📦',
            'Customer': '👤',
            'Invoice': '🧾',
            'Supplier': '🏢',
            'Batch': '🏷️'
        };

        const icon = iconMap[hit.entityType] || '🔍';
        const primaryAction = hit.actions && hit.actions.length > 0 ? hit.actions[0] : null;
        const targetUrl = primaryAction?.targetUrl || '#';

        const itemEl = document.createElement('div');
        itemEl.className = 'cmd-item';
        itemEl.setAttribute('data-type', hit.entityType.toLowerCase());
        itemEl.setAttribute('data-url', targetUrl);

        itemEl.innerHTML = `
            <span class="cmd-item-icon">${icon}</span>
            <div class="cmd-item-info">
                <div class="cmd-item-title">${escapeHtml(hit.title)}</div>
                <div class="cmd-item-desc">${escapeHtml(hit.subtitle || hit.code)} • ${escapeHtml(hit.entityType)}</div>
            </div>
            <span class="cmd-badge">${escapeHtml(primaryAction?.label || 'View')}</span>
        `;

        dynamicList.appendChild(itemEl);

        // Additional actions if present
        if (hit.actions && hit.actions.length > 1) {
            for (let i = 1; i < hit.actions.length; i++) {
                const act = hit.actions[i];
                const subItem = document.createElement('div');
                subItem.className = 'cmd-item';
                subItem.setAttribute('data-type', 'action');
                subItem.setAttribute('data-url', act.targetUrl);
                subItem.innerHTML = `
                    <span class="cmd-item-icon">⚡</span>
                    <div class="cmd-item-info">
                        <div class="cmd-item-title">${escapeHtml(act.label)}</div>
                        <div class="cmd-item-desc">Action for ${escapeHtml(hit.title)}</div>
                    </div>
                    <span class="cmd-badge">Action</span>
                `;
                dynamicList.appendChild(subItem);
            }
        }
    }

    function renderNoHit() {
        if (!dynamicGroup || !dynamicList) return;
        dynamicGroup.style.display = 'block';
        dynamicList.innerHTML = `
            <div class="cmd-item" style="cursor: default; opacity: 0.6;">
                <span class="cmd-item-icon">🔍</span>
                <div class="cmd-item-info">
                    <div class="cmd-item-title">No matching records found</div>
                    <div class="cmd-item-desc">Try searching by product barcode, invoice ID, or customer phone</div>
                </div>
            </div>
        `;
    }

    function handleKeyNavigation(e) {
        const visibleItems = Array.from(overlay.querySelectorAll('.cmd-item')).filter(
            item => item.offsetParent !== null && item.getAttribute('data-url')
        );

        if (visibleItems.length === 0) return;

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            selectedIndex = (selectedIndex + 1) % visibleItems.length;
            updateSelection(selectedIndex);
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            selectedIndex = (selectedIndex - 1 + visibleItems.length) % visibleItems.length;
            updateSelection(selectedIndex);
        } else if (e.key === 'Enter') {
            e.preventDefault();
            const target = visibleItems[selectedIndex];
            if (target) {
                const url = target.getAttribute('data-url');
                if (url && url !== '#') {
                    window.location.href = url;
                }
            }
        }
    }

    function updateSelection(index) {
        const visibleItems = Array.from(overlay.querySelectorAll('.cmd-item')).filter(
            item => item.offsetParent !== null && item.getAttribute('data-url')
        );

        visibleItems.forEach((item, idx) => {
            if (idx === index) {
                item.classList.add('selected');
                item.scrollIntoView({ block: 'nearest' });
            } else {
                item.classList.remove('selected');
            }
        });
        selectedIndex = index;
    }

    function escapeHtml(str) {
        if (!str) return '';
        const div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }

    // Auto initialize on DOMContentLoaded
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initCommandPalette);
    } else {
        initCommandPalette();
    }
})();
