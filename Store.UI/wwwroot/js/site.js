(() => {
    // --- Escape HTML for toast messages (XSS-safe) ---
    const escapeHtml = (text) => String(text ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');

    // --- Unified modal system (legacy .modalView + .modal-overlay) ---
    const closeAllModals = () => {
        document.querySelectorAll('.modalView.show').forEach(m => m.classList.remove('show'));
        document.querySelectorAll('.modal-overlay:not([hidden])').forEach(m => {
            // Keep global crop/viewer open state managed separately unless explicitly closed
            if (m.id === 'globalCropModal' || m.id === 'globalImageViewerModal') return;
            m.hidden = true;
        });
    };

    window.openModal = (id) => {
        if (!id) return;
        const target = document.getElementById(id);
        if (!target) return;
        if (target.classList.contains('modal-overlay')) {
            target.hidden = false;
        } else {
            closeAllModals();
            target.classList.add('show');
        }
    };

    window.closeModal = (id) => {
        if (!id) {
            closeAllModals();
            return;
        }
        const target = document.getElementById(id);
        if (!target) return;
        if (target.classList.contains('modal-overlay')) {
            target.hidden = true;
        } else {
            target.classList.remove('show');
        }
    };

    document.addEventListener('click', (e) => {
        const openBtn = e.target.closest('[data-open-modal]');
        if (openBtn) {
            e.preventDefault();
            const id = openBtn.getAttribute('data-open-modal');
            window.openModal(id);
            return;
        }

        const closeBtn = e.target.closest('[data-close-modal]');
        if (closeBtn) {
            e.preventDefault();
            const id = closeBtn.getAttribute('data-close-modal');
            window.closeModal(id || undefined);
            return;
        }

        // Click on overlay background closes modal
        if (e.target.classList?.contains('modal-overlay') &&
            e.target.id !== 'globalCropModal' &&
            e.target.id !== 'globalImageViewerModal') {
            e.target.hidden = true;
        }
    });

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            closeAllModals();
            // Close any open blades
            document.querySelectorAll('.blade.open').forEach(blade => {
                window.closeBlade?.(blade.id);
            });
        }
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

        const safeType = type === 'error' ? 'error' : 'success';
        const toast = document.createElement('div');
        toast.className = `toast toast-${safeType}`;
        toast.setAttribute('role', 'status');

        const icon = safeType === 'success' ? '✓' : '!';

        toast.innerHTML = `
            <div class="toast-icon" aria-hidden="true">${icon}</div>
            <div class="toast-message">${escapeHtml(message)}</div>
            <button type="button" class="toast-close" aria-label="Close">&times;</button>
        `;

        container.appendChild(toast);

        requestAnimationFrame(() => {
            toast.classList.add('show');
        });

        const dismiss = () => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        };

        toast.querySelector('.toast-close')?.addEventListener('click', dismiss);
        setTimeout(dismiss, 5000);
    };

    // Auto-surface server TempData / status banners as toasts
    document.querySelectorAll('[data-toast-message]').forEach(el => {
        const msg = el.getAttribute('data-toast-message');
        if (!msg) return;
        const type = el.getAttribute('data-toast-type') === 'error' ? 'error' : 'success';
        window.showToast(type, msg);
        el.remove();
    });

    // --- Blades ---
    window.openBlade = (id) => {
        const blade = document.getElementById(id);
        if (!blade) return;
        const overlayId = blade.getAttribute('data-overlay-id');
        if (overlayId) {
            const overlay = document.getElementById(overlayId);
            if (overlay) overlay.classList.add('open');
        }
        // Also support overlay linked via data-blade-id
        document.querySelectorAll(`.blade-overlay[data-blade-id="${id}"]`).forEach(o => o.classList.add('open'));
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
        document.querySelectorAll(`.blade-overlay[data-blade-id="${id}"]`).forEach(o => o.classList.remove('open'));
        blade.classList.remove('open');
    };

    document.addEventListener('click', (e) => {
        const openBladeBtn = e.target.closest('[data-open-blade]');
        if (openBladeBtn) {
            e.preventDefault();
            window.openBlade(openBladeBtn.getAttribute('data-open-blade'));
            return;
        }

        const closeBladeBtn = e.target.closest('[data-close-blade]');
        if (closeBladeBtn) {
            e.preventDefault();
            window.closeBlade(closeBladeBtn.getAttribute('data-close-blade'));
            return;
        }

        const overlay = e.target.closest('.blade-overlay');
        if (overlay && e.target === overlay) {
            const bladeId = overlay.getAttribute('data-blade-id')
                || document.querySelector(`.blade[data-overlay-id="${overlay.id}"]`)?.id;
            if (bladeId) window.closeBlade(bladeId);
        }
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

    // --- Image Compression Utility ---
    window.compressImage = async (file, options = {}) => {
        if (!window.imageCompression) {
            console.warn('browser-image-compression library not loaded.');
            return file;
        }

        const defaultOptions = {
            maxSizeMB: 1,
            maxWidthOrHeight: 1024,
            useWebWorker: true,
            fileType: 'image/webp',
            initialQuality: 0.8
        };

        const mergedOptions = { ...defaultOptions, ...options };

        try {
            console.log(`Original file size: ${file.size / 1024 / 1024} MB`);
            const compressedFile = await window.imageCompression(file, mergedOptions);
            console.log(`Compressed file size: ${compressedFile.size / 1024 / 1024} MB`);
            return compressedFile;
        } catch (error) {
            console.error('Error during image compression:', error);
            // Return original file if compression fails
            return file;
        }
    };

    // --- Image Cropping Utility ---
    let currentCropper = null;
    let currentCropInput = null;

    document.addEventListener('change', (e) => {
        if (e.target.matches('input[type="file"][data-crop="true"]')) {
            const input = e.target;
            if (input.files && input.files.length > 0) {
                const file = input.files[0];
                if (file.type.startsWith('image/')) {
                    const reader = new FileReader();
                    reader.onload = (event) => {
                        const modal = document.getElementById('globalCropModal');
                        const img = document.getElementById('cropperImage');
                        
                        if (currentCropper) {
                            currentCropper.destroy();
                            currentCropper = null;
                        }
                        
                        modal.hidden = false;
                        currentCropInput = input;

                        img.onload = () => {
                            const aspectRatio = input.dataset.cropAspectRatio ? parseFloat(input.dataset.cropAspectRatio) : 1;
                            currentCropper = new Cropper(img, {
                                aspectRatio: aspectRatio,
                                viewMode: 1,
                                autoCropArea: 0.8,
                                background: false
                            });
                        };
                        
                        img.src = event.target.result;
                    };
                    reader.readAsDataURL(file);
                }
            }
        }
    });

    const closeCropModal = () => {
        const modal = document.getElementById('globalCropModal');
        if (modal) modal.hidden = true;
        if (currentCropper) {
            currentCropper.destroy();
            currentCropper = null;
        }
        currentCropInput = null;
    };

    const applyCrop = () => {
        if (currentCropper && currentCropInput) {
            const data = currentCropper.getData(true);
            const form = currentCropInput.closest('form');
            if (form) {
                const setHiddenInput = (name, value) => {
                    let hidden = form.querySelector(`input[name="${name}"]`);
                    if (!hidden) {
                        hidden = document.createElement('input');
                        hidden.type = 'hidden';
                        hidden.name = name;
                        form.appendChild(hidden);
                    }
                    hidden.value = value;
                };

                setHiddenInput('CropX', data.x);
                setHiddenInput('CropY', data.y);
                setHiddenInput('CropW', data.width);
                setHiddenInput('CropH', data.height);
            }
            closeCropModal();
        }
    };

    document.getElementById('btnCancelCrop')?.addEventListener('click', () => {
        if (currentCropInput) {
            currentCropInput.value = ''; // Reset file input
        }
        closeCropModal();
    });
    document.getElementById('btnCancelCrop2')?.addEventListener('click', () => {
        if (currentCropInput) {
            currentCropInput.value = ''; // Reset file input
        }
        closeCropModal();
    });
    document.getElementById('btnApplyCrop')?.addEventListener('click', applyCrop);

    // --- Global Form Image Interceptor ---
    document.addEventListener('submit', async (e) => {
        const form = e.target;
        if (form.dataset.compressed === "true") return; // already processed

        const fileInputs = form.querySelectorAll('input[type="file"][accept*="image"]');
        let hasFilesToCompress = false;
        
        fileInputs.forEach(input => {
            if (input.files && input.files.length > 0) {
                hasFilesToCompress = true;
            }
        });

        if (hasFilesToCompress) {
            e.preventDefault(); // stop normal submission

            const submitBtn = form.querySelector('[type="submit"]');
            const originalBtnText = submitBtn ? submitBtn.textContent : '';
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.textContent = 'Compressing...';
            }

            try {
                for (let input of fileInputs) {
                    if (input.files && input.files.length > 0) {
                        const originalFile = input.files[0];
                        const compressedFile = await window.compressImage(originalFile);
                        
                        const dataTransfer = new DataTransfer();
                        let newFileName = originalFile.name.replace(/\.[^/.]+$/, "") + ".webp";
                        dataTransfer.items.add(new File([compressedFile], newFileName, { type: compressedFile.type }));
                        
                        input.files = dataTransfer.files;
                    }
                }
                
                // Mark as processed and submit
                form.dataset.compressed = "true";
                form.submit();
            } catch (err) {
                console.error("Image compression failed during form submission", err);
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.textContent = originalBtnText;
                }
                window.showToast?.('error', 'Failed to compress image before uploading.');
            }
        }
    });

    // --- Global Image Viewer Utility ---
    window.viewImage = (url) => {
        if (!url) return;
        const modal = document.getElementById('globalImageViewerModal');
        const img = document.getElementById('globalImageViewerImage');
        if (modal && img) {
            img.src = url;
            modal.hidden = false;
        }
    };

    window.closeImageViewer = () => {
        const modal = document.getElementById('globalImageViewerModal');
        const img = document.getElementById('globalImageViewerImage');
        if (modal && img) {
            modal.hidden = true;
            img.src = '';
        }
    };

    // Close viewer when clicking on the overlay background
    document.getElementById('globalImageViewerModal')?.addEventListener('click', (e) => {
        if (e.target.id === 'globalImageViewerModal') {
            window.closeImageViewer();
        }
    });

    // --- Smart Lookup Component Engine ---
    window.SmartLookup = {
        attach: function(container, options = {}) {
            if (!container || container.dataset.smartLookupInitialized) return;
            container.dataset.smartLookupInitialized = "true";

            const hiddenInput = container.querySelector('input[type="hidden"]') || container.querySelector('[data-smart-hidden]');
            let textInput = container.querySelector('.smart-lookup-input');
            let clearBtn = container.querySelector('.smart-lookup-clear');
            let dropdown = container.querySelector('.smart-lookup-dropdown');

            if (!textInput) {
                textInput = document.createElement('input');
                textInput.type = 'text';
                textInput.className = 'smart-lookup-input';
                textInput.placeholder = options.placeholder || container.dataset.placeholder || 'Type to search...';
                textInput.autocomplete = 'off';
                container.appendChild(textInput);
            }

            if (!clearBtn) {
                clearBtn = document.createElement('button');
                clearBtn.type = 'button';
                clearBtn.className = 'smart-lookup-clear';
                clearBtn.innerHTML = '&times;';
                clearBtn.title = 'Clear selection';
                container.appendChild(clearBtn);
            }

            if (!dropdown) {
                dropdown = document.createElement('div');
                dropdown.className = 'smart-lookup-dropdown';
                container.appendChild(dropdown);
            }

            const fetchUrl = options.url || container.dataset.url || '';
            const minChars = options.minChars !== undefined ? options.minChars : (container.dataset.minChars ? parseInt(container.dataset.minChars, 10) : 1);
            const onSelect = options.onSelect || (window[container.dataset.onSelect]);
            const onClear = options.onClear || (window[container.dataset.onClear]);

            let abortCtrl = null;
            let debounceTimer = null;
            let currentItems = [];
            let activeIndex = -1;

            const updateHasValue = () => {
                if (hiddenInput && hiddenInput.value) {
                    container.classList.add('has-value');
                } else {
                    container.classList.remove('has-value');
                }
            };
            updateHasValue();

            const closeDropdown = () => {
                dropdown.classList.remove('show');
                dropdown.innerHTML = '';
                activeIndex = -1;
            };

            const highlightItem = (index) => {
                const items = dropdown.querySelectorAll('.smart-lookup-item');
                items.forEach((it, idx) => {
                    if (idx === index) {
                        it.classList.add('active');
                        it.scrollIntoView({ block: 'nearest' });
                    } else {
                        it.classList.remove('active');
                    }
                });
            };

            const renderItems = (items) => {
                currentItems = items || [];
                activeIndex = -1;
                dropdown.innerHTML = '';

                if (!currentItems.length) {
                    dropdown.innerHTML = '<div class="smart-lookup-empty">No matching results found</div>';
                    dropdown.classList.add('show');
                    return;
                }

                currentItems.forEach((item, index) => {
                    const el = document.createElement('div');
                    el.className = 'smart-lookup-item';
                    el.dataset.index = index;

                    const title = escapeHtml(item.title || item.name || item.fullName || item.label || item.id || '');
                    const sub = escapeHtml(item.sub || item.subtitle || item.description || item.code || '');
                    const badge = item.badge ? `<span class="smart-lookup-badge">${escapeHtml(item.badge)}</span>` : '';

                    el.innerHTML = `
                        <div class="smart-lookup-item-header">
                            <span class="smart-lookup-item-title">${title}</span>
                            ${badge}
                        </div>
                        ${sub ? `<div class="smart-lookup-item-sub">${sub}</div>` : ''}
                    `;

                    el.addEventListener('click', (e) => {
                        e.stopPropagation();
                        selectItem(item);
                    });

                    dropdown.appendChild(el);
                });

                dropdown.classList.add('show');
            };

            const selectItem = (item) => {
                const val = item.id !== undefined ? item.id : (item.value !== undefined ? item.value : '');
                const label = item.title || item.name || item.fullName || item.label || val;

                if (hiddenInput) {
                    hiddenInput.value = val;
                    hiddenInput.dispatchEvent(new Event('change', { bubbles: true }));
                }

                textInput.value = label;
                updateHasValue();
                closeDropdown();

                if (typeof onSelect === 'function') {
                    onSelect(item, container);
                }
            };

            const clearSelection = () => {
                if (hiddenInput) {
                    hiddenInput.value = '';
                    hiddenInput.dispatchEvent(new Event('change', { bubbles: true }));
                }
                textInput.value = '';
                updateHasValue();
                closeDropdown();

                if (typeof onClear === 'function') {
                    onClear(container);
                }
            };

            clearBtn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                clearSelection();
                textInput.focus();
            });

            textInput.addEventListener('input', (e) => {
                const query = textInput.value.trim();

                // If user edits text after selecting an item, invalidate previous ID
                if (hiddenInput && hiddenInput.value && query !== textInput.dataset.selectedLabel) {
                    hiddenInput.value = '';
                    updateHasValue();
                }

                clearTimeout(debounceTimer);
                if (query.length < minChars) {
                    closeDropdown();
                    return;
                }

                dropdown.innerHTML = `
                    <div class="smart-lookup-loading">
                        <div class="smart-lookup-spinner"></div>
                        <span>Searching...</span>
                    </div>
                `;
                dropdown.classList.add('show');

                debounceTimer = setTimeout(async () => {
                    if (abortCtrl) abortCtrl.abort();
                    abortCtrl = new AbortController();

                    try {
                        let finalUrl = fetchUrl;
                        if (!finalUrl) {
                            const lookupType = container.dataset.lookupType || 'items';
                            finalUrl = `?handler=Search${lookupType.charAt(0).toUpperCase() + lookupType.slice(1)}`;
                        }

                        const separator = finalUrl.includes('?') ? '&' : '?';
                        const reqUrl = `${finalUrl}${separator}q=${encodeURIComponent(query)}`;

                        const res = await fetch(reqUrl, {
                            signal: abortCtrl.signal,
                            headers: { 'X-Requested-With': 'XMLHttpRequest' }
                        });

                        if (!res.ok) {
                            dropdown.innerHTML = '<div class="smart-lookup-empty">Error searching. Please try again.</div>';
                            return;
                        }

                        const data = await res.json();
                        renderItems(Array.isArray(data) ? data : (data.items || []));
                    } catch (err) {
                        if (err.name !== 'AbortError') {
                            console.error('Smart lookup search error:', err);
                            dropdown.innerHTML = '<div class="smart-lookup-empty">Search failed.</div>';
                        }
                    }
                }, 220);
            });

            textInput.addEventListener('keydown', (e) => {
                if (!dropdown.classList.contains('show')) {
                    if (e.key === 'ArrowDown') {
                        textInput.dispatchEvent(new Event('input'));
                    }
                    return;
                }

                if (e.key === 'ArrowDown') {
                    e.preventDefault();
                    if (currentItems.length > 0) {
                        activeIndex = (activeIndex + 1) % currentItems.length;
                        highlightItem(activeIndex);
                    }
                } else if (e.key === 'ArrowUp') {
                    e.preventDefault();
                    if (currentItems.length > 0) {
                        activeIndex = (activeIndex - 1 + currentItems.length) % currentItems.length;
                        highlightItem(activeIndex);
                    }
                } else if (e.key === 'Enter') {
                    if (activeIndex >= 0 && activeIndex < currentItems.length) {
                        e.preventDefault();
                        e.stopPropagation();
                        selectItem(currentItems[activeIndex]);
                    }
                } else if (e.key === 'Escape') {
                    closeDropdown();
                }
            });

            // Auto-select text on focus so subsequent barcode scanning/typing replaces existing value
            textInput.addEventListener('focus', () => {
                textInput.select();
                if (textInput.value.trim().length >= minChars && !dropdown.classList.contains('show')) {
                    textInput.dispatchEvent(new Event('input'));
                }
            });
        },

        initAll: function(root = document) {
            root.querySelectorAll('.smart-lookup-wrap, [data-smart-lookup]').forEach(el => {
                window.SmartLookup.attach(el);
            });
        }
    };

    // Close all open dropdowns on outside click
    document.addEventListener('click', (e) => {
        if (!e.target.closest('.smart-lookup-wrap')) {
            document.querySelectorAll('.smart-lookup-dropdown.show').forEach(d => d.classList.remove('show'));
        }
    });

    // ── Universal Barcode Input Guard (Anti-Duplicate & Anti-Append) ────────
    window.BarcodeGuard = {
        DEFAULT_COOLDOWN_MS: 800, // Cooldown to ignore duplicate scans of identical barcode
        lastScannedCode: '',
        lastScannedTime: 0,

        /**
         * Checks if a scanned barcode is a duplicate within the cooldown window.
         * @param {string} rawCode
         * @param {number} cooldownMs
         * @returns {boolean} True if accepted (new scan), False if duplicate (suppressed)
         */
        acceptScan: function(rawCode, cooldownMs = this.DEFAULT_COOLDOWN_MS) {
            const code = String(rawCode ?? '').trim();
            if (!code) return false;

            const now = Date.now();
            if (code === this.lastScannedCode && (now - this.lastScannedTime) < cooldownMs) {
                console.warn(`[BarcodeGuard] Suppressed duplicate bounce for "${code}" (${now - this.lastScannedTime}ms since last read).`);
                if (window.showToast) {
                    window.showToast('error', `Duplicate scan ignored: ${code}`);
                }
                return false;
            }

            this.lastScannedCode = code;
            this.lastScannedTime = now;
            return true;
        },

        /**
         * Attaches anti-append and anti-duplicate guards to a specific text input element.
         * @param {HTMLInputElement} inputEl
         * @param {Object} options
         */
        attach: function(inputEl, options = {}) {
            if (!inputEl || inputEl.dataset.barcodeGuarded === 'true') return;
            inputEl.dataset.barcodeGuarded = 'true';

            const cooldown = options.cooldownMs ?? this.DEFAULT_COOLDOWN_MS;
            const autoClear = options.autoClear ?? (inputEl.dataset.barcodeAutoClear !== 'false');
            let lastKeyTime = 0;

            // 1. Auto-select existing text on focus so subsequent scanning or typing overwrites rather than appends
            inputEl.addEventListener('focus', () => {
                inputEl.select();
            });

            // 2. Detect hardware scanner burst start: if rapid keys arrive after a pause and text isn't selected, replace buffer
            inputEl.addEventListener('keydown', (e) => {
                const now = Date.now();
                const interval = now - lastKeyTime;
                lastKeyTime = now;

                // Enter key terminates a barcode scan
                if (e.key === 'Enter') {
                    const value = inputEl.value.trim();
                    if (!value) return;

                    // If it's a duplicate scan from lingering under the laser, stop submission
                    if (!window.BarcodeGuard.acceptScan(value, cooldown)) {
                        e.preventDefault();
                        e.stopPropagation();
                        if (autoClear) inputEl.value = '';
                        return;
                    }

                    // Valid scan: if autoClear is requested, clear immediately so future scans never concatenate
                    if (autoClear) {
                        setTimeout(() => {
                            if (document.activeElement === inputEl) {
                                inputEl.value = '';
                            }
                        }, 50);
                    }
                }
            });
        },

        /**
         * Scans the document and attaches guards to all barcode-related inputs.
         * @param {HTMLElement|Document} root
         */
        initAll: function(root = document) {
            const selector = [
                'input[data-barcode-input]',
                'input[id*="barcode" i]',
                'input[id*="Barcode" i]',
                'input[name*="barcode" i]',
                'input[name*="Barcode" i]',
                'input.barcode-input'
            ].join(', ');

            root.querySelectorAll(selector).forEach(input => {
                this.attach(input);
            });
        }
    };

    // Auto-init on page load
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            window.SmartLookup.initAll();
            window.BarcodeGuard.initAll();
        });
    } else {
        window.SmartLookup.initAll();
        window.BarcodeGuard.initAll();
    }

})();

