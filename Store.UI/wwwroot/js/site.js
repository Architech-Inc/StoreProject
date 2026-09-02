(() => {
    // --- Escape HTML for toast messages (XSS-safe) ---
    const escapeHtml = (text) => String(text ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');

    // --- Unified modal system (legacy .modalView + .modal-overlay) ---
    const focusRestoreMap = new WeakMap();
    const focusableSelector = 'a[href], button:not([disabled]), textarea:not([disabled]), input:not([disabled]), select:not([disabled]), [tabindex]:not([tabindex="-1"])';

    const getFocusableElements = (container) => {
        if (!container) return [];
        return [...container.querySelectorAll(focusableSelector)]
            .filter(el => !el.hasAttribute('hidden') && !el.closest('[hidden]') && !el.disabled && el.offsetParent !== null);
    };

    const restoreFocus = (target) => {
        if (target && typeof target.focus === 'function') {
            requestAnimationFrame(() => target.focus());
        }
    };

    const closeAllModals = () => {
        document.querySelectorAll('.modalView.show').forEach(m => m.classList.remove('show'));
        document.querySelectorAll('.modal-overlay:not([hidden])').forEach(m => {
            // Keep global crop/viewer open state managed separately unless explicitly closed
            if (m.id === 'globalCropModal' || m.id === 'globalImageViewerModal') return;
            const restoreTarget = focusRestoreMap.get(m);
            if (restoreTarget) restoreFocus(restoreTarget);
            focusRestoreMap.delete(m);
            m.hidden = true;
        });
    };

    window.openModal = (id) => {
        if (!id) return;
        const target = document.getElementById(id);
        if (!target) return;
        focusRestoreMap.set(target, document.activeElement instanceof HTMLElement ? document.activeElement : null);
        if (target.classList.contains('modal-overlay')) {
            target.hidden = false;
            const firstFocusable = getFocusableElements(target)[0];
            if (firstFocusable) firstFocusable.focus();
        } else {
            closeAllModals();
            target.classList.add('show');
            const firstFocusable = getFocusableElements(target)[0];
            if (firstFocusable) firstFocusable.focus();
        }
    };

    window.closeModal = (id) => {
        if (!id) {
            closeAllModals();
            return;
        }
        const target = document.getElementById(id);
        if (!target) return;
        const restoreTarget = focusRestoreMap.get(target);
        if (restoreTarget) restoreFocus(restoreTarget);
        focusRestoreMap.delete(target);
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

    const trapFocusInDialog = (event) => {
        if (event.key !== 'Tab') return;

        const openDialog = document.querySelector('.modal-overlay:not([hidden]), .modalView.show, .blade.open');
        if (!openDialog) return;

        const focusable = getFocusableElements(openDialog);
        if (!focusable.length) {
            event.preventDefault();
            openDialog.focus?.();
            return;
        }

        const first = focusable[0];
        const last = focusable[focusable.length - 1];
        if (event.shiftKey && document.activeElement === first) {
            event.preventDefault();
            last.focus();
            return;
        }

        if (!event.shiftKey && document.activeElement === last) {
            event.preventDefault();
            first.focus();
            return;
        }

        if (!openDialog.contains(document.activeElement)) {
            event.preventDefault();
            first.focus();
        }
    };

    document.addEventListener('keydown', (e) => {
        trapFocusInDialog(e);
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
    window.copyTextToClipboard = async (text, options = {}) => {
        const {
            successMessage = 'Copied to clipboard.',
            errorMessage = 'Unable to copy to clipboard.',
            target = null,
            successText = '✓ Copied!'
        } = options;

        try {
            if (!text && text !== 0) {
                throw new Error('No text provided');
            }
            await navigator.clipboard.writeText(String(text));

            if (target && typeof target.focus === 'function') {
                const previousText = target.dataset.originalLabel || target.textContent;
                target.dataset.originalLabel = previousText;
                target.textContent = successText;
                target.classList.add('is-copied');
                setTimeout(() => {
                    target.textContent = previousText;
                    target.classList.remove('is-copied');
                }, 1800);
            }

            window.showToast?.('success', successMessage);
            return true;
        } catch (error) {
            console.error('Clipboard copy failed:', error);
            window.showToast?.('error', errorMessage);
            return false;
        }
    };

    window.showToast = (type, message, options = {}) => {
        const container = document.getElementById('toast-container');
        if (!container) return;

        const allowedTypes = ['success', 'error', 'warning', 'info'];
        const safeType = allowedTypes.includes(type) ? type : 'info';
        const toast = document.createElement('div');
        toast.className = `toast toast-${safeType}`;
        toast.setAttribute('role', 'status');
        toast.setAttribute('aria-live', 'polite');

        const iconMap = { success: '✓', error: '!', warning: '⚠', info: 'i' };
        const icon = iconMap[safeType] ?? 'i';
        const actionLabel = options.actionLabel || '';
        const actionHandler = typeof options.actionHandler === 'function' ? options.actionHandler : null;
        const duration = Number.isFinite(options.duration) ? options.duration : 5000;

        toast.innerHTML = `
            <div class="toast-icon" aria-hidden="true">${icon}</div>
            <div class="toast-message">${escapeHtml(message)}</div>
            ${actionLabel ? `<button type="button" class="toast-action" aria-label="${escapeHtml(actionLabel)}">${escapeHtml(actionLabel)}</button>` : ''}
            <button type="button" class="toast-close" aria-label="Close">&times;</button>
        `;

        container.appendChild(toast);

        requestAnimationFrame(() => {
            toast.classList.add('show');
        });

        const dismiss = () => {
            if (!toast.isConnected) return;
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        };

        toast.addEventListener('mouseenter', () => {
            if (toast.dataset.dismissTimer) {
                clearTimeout(Number(toast.dataset.dismissTimer));
            }
        });

        toast.addEventListener('mouseleave', () => {
            if (toast.dataset.dismissTimer) {
                toast.dataset.dismissTimer = String(setTimeout(dismiss, 1500));
            }
        });

        toast.querySelector('.toast-close')?.addEventListener('click', dismiss);
        const actionButton = toast.querySelector('.toast-action');
        if (actionButton && actionHandler) {
            actionButton.addEventListener('click', () => {
                actionHandler();
                dismiss();
            });
        }

        toast.dataset.dismissTimer = String(setTimeout(dismiss, duration));
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
        focusRestoreMap.set(blade, document.activeElement instanceof HTMLElement ? document.activeElement : null);
        const overlayId = blade.getAttribute('data-overlay-id');
        if (overlayId) {
            const overlay = document.getElementById(overlayId);
            if (overlay) overlay.classList.add('open');
        }
        // Also support overlay linked via data-blade-id
        document.querySelectorAll(`.blade-overlay[data-blade-id="${id}"]`).forEach(o => o.classList.add('open'));
        blade.classList.add('open');
        const firstFocusable = getFocusableElements(blade)[0];
        if (firstFocusable) firstFocusable.focus();
    };

    window.closeBlade = (id) => {
        const blade = document.getElementById(id);
        if (!blade) return;
        const restoreTarget = focusRestoreMap.get(blade);
        if (restoreTarget) restoreFocus(restoreTarget);
        focusRestoreMap.delete(blade);
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

                if (currentCropInput.dataset.cropAutoSubmit === 'true') {
                    form.submit();
                }
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

    /**
     * ============================================================
     * BarcodeScannerService - Background Sleeper Daemon & Action Hub
     * ============================================================
     */
    window.BarcodeScannerService = {
        STORAGE_KEY: 'clexan_scanner_pref',
        MAX_BURST_INTERVAL_MS: 40,
        MIN_BURST_LENGTH: 3,
        
        state: {
            status: 'active', // 'active' | 'paused' | 'session_disabled' | 'permanently_disabled' | 'in_page_only'
            pauseUntil: null
        },
        
        buffer: [],
        lastKeyTime: 0,
        currentModalResult: null,

        init: function() {
            // Gatekeeper: strict authentication check
            const path = window.location.pathname.toLowerCase();
            const isPublicPage = path.includes('/login') || path.includes('/register') || path.includes('/forgotpassword') || path.includes('/resetpassword') || path.includes('/accessdenied') || path.includes('/error');
            const isAuthenticated = document.body.dataset.authenticated === 'true' || (document.getElementById('appShell') !== null && !isPublicPage);
            const fab = document.getElementById('scannerFabWidget');
            if (!isAuthenticated || isPublicPage) {
                if (fab) fab.style.display = 'none';
                return;
            }
            if (fab) fab.style.display = '';

            this.loadPreferences();
            this.setupGlobalKeyListener();
            this.setupFabControls();
            this.setupModalEvents();
            this.updateFabUI();
            
            // Background check for pause expiration every 30s
            setInterval(() => this.checkPauseExpiration(), 30000);
        },

        loadPreferences: function() {
            try {
                // Check session-level preference first
                const sessionPref = sessionStorage.getItem(this.STORAGE_KEY);
                if (sessionPref === 'session_disabled') {
                    this.state.status = 'session_disabled';
                    return;
                }

                // Check local storage preferences
                const saved = localStorage.getItem(this.STORAGE_KEY);
                if (saved) {
                    const parsed = JSON.parse(saved);
                    if (parsed.status === 'paused' && parsed.pauseUntil && parsed.pauseUntil > Date.now()) {
                        this.state.status = 'paused';
                        this.state.pauseUntil = parsed.pauseUntil;
                    } else if (parsed.status === 'permanently_disabled' || parsed.status === 'in_page_only') {
                        this.state.status = parsed.status;
                    } else {
                        this.state.status = 'active';
                    }
                }
            } catch (e) {
                this.state.status = 'active';
            }
        },

        savePreferences: function() {
            try {
                if (this.state.status === 'session_disabled') {
                    sessionStorage.setItem(this.STORAGE_KEY, 'session_disabled');
                } else {
                    sessionStorage.removeItem(this.STORAGE_KEY);
                    localStorage.setItem(this.STORAGE_KEY, JSON.stringify({
                        status: this.state.status,
                        pauseUntil: this.state.pauseUntil
                    }));
                }
            } catch (e) {}
            this.updateFabUI();
        },

        checkPauseExpiration: function() {
            if (this.state.status === 'paused' && this.state.pauseUntil && Date.now() >= this.state.pauseUntil) {
                this.state.status = 'active';
                this.state.pauseUntil = null;
                this.savePreferences();
                if (window.showToast) window.showToast('info', 'Smart Scanner has automatically resumed.');
            }
        },

        setStatus: function(newStatus, durationMs = 0) {
            this.state.status = newStatus;
            this.state.pauseUntil = durationMs > 0 ? Date.now() + durationMs : null;
            this.savePreferences();
        },

        /**
         * Returns true if the currently focused element is a text-accepting input,
         * textarea, select, or contenteditable. Used to suppress global scanner
         * buffering when the user or scanner is deliberately targeting a page input.
         */
        isTextInputFocused: function() {
            const el = document.activeElement;
            if (!el || el === document.body) return false;
            const tag = el.tagName;
            if (tag === 'TEXTAREA' || tag === 'SELECT') return true;
            if (tag === 'INPUT') {
                // Exclude types that don't accept text
                const nonTextTypes = ['button', 'submit', 'reset', 'checkbox', 'radio', 'file', 'image', 'range', 'color'];
                return !nonTextTypes.includes((el.type || 'text').toLowerCase());
            }
            if (el.isContentEditable) return true;
            return false;
        },

        setupGlobalKeyListener: function() {
            window.addEventListener('keydown', (e) => {
                // If scanner disabled or session paused, ignore keystrokes
                if (this.state.status === 'session_disabled' || this.state.status === 'permanently_disabled') return;
                if (this.state.status === 'paused') {
                    this.checkPauseExpiration();
                    if (this.state.status === 'paused') return;
                }

                // Ignore control keys, function keys, meta keys
                if (e.ctrlKey || e.altKey || e.metaKey || e.key.length > 1 && e.key !== 'Enter') return;

                // ── KEY FIX ──────────────────────────────────────────────────────────
                // If any text-accepting element (input, textarea, select, contenteditable)
                // currently has focus, the browser is already routing keystrokes into it.
                // Do NOT buffer anything — let the element handle the scan natively.
                // This prevents double-entry and prevents the smart-scan modal from
                // hijacking barcode scanners that are aimed at a focused page input.
                if (this.isTextInputFocused()) {
                    // Still reset our buffer so we don't accumulate stale data
                    this.buffer = [];
                    return;
                }
                // ─────────────────────────────────────────────────────────────────────

                const now = Date.now();
                const interval = now - this.lastKeyTime;
                this.lastKeyTime = now;

                // If interval exceeds burst threshold, reset buffer
                if (interval > 120 && this.buffer.length > 0) {
                    this.buffer = [];
                }

                if (e.key === 'Enter') {
                    if (this.buffer.length >= this.MIN_BURST_LENGTH) {
                        // Analyze inter-keystroke intervals
                        const totalTime = this.buffer[this.buffer.length - 1].time - this.buffer[0].time;
                        const avgInterval = totalTime / (this.buffer.length - 1 || 1);

                        // If average interval is fast enough (<= 45ms), consider it a hardware scanner burst
                        if (avgInterval <= 45 || (this.buffer.length >= 5 && avgInterval <= 65)) {
                            const scannedCode = this.buffer.map(b => b.char).join('').trim();
                            this.buffer = [];

                            if (scannedCode.length >= 2) {
                                e.preventDefault();
                                e.stopPropagation();
                                this.handleHardwareScan(scannedCode);
                            }
                        }
                    }
                    this.buffer = [];
                } else if (e.key.length === 1) {
                    this.buffer.push({ char: e.key, time: now });
                }
            }, true);
        },

        handleHardwareScan: function(code) {
            // Deduplication guard
            if (window.BarcodeGuard && !window.BarcodeGuard.acceptScan(code, 800)) {
                return;
            }

            // Note: by the time we reach here, isTextInputFocused() was already false
            // in setupGlobalKeyListener, so we never double-inject into a focused input.
            // The only remaining job is to route to a *visible* page barcode input
            // (if exactly one is available) or fall back to the global modal.

            // 1. Check for in-page barcode target inputs that are visible and ready
            //    Selector covers: explicit data-scanner-input attribute, id/name containing
            //    "barcode", and the .barcode-input CSS class.
            const pageBarcodeInputs = Array.from(document.querySelectorAll(
                'input[data-scanner-input], input[data-barcode-input], input[id*="barcode" i], input[name*="barcode" i], input.barcode-input'
            )).filter(el => el.offsetParent !== null && !el.disabled && !el.readOnly);

            if (pageBarcodeInputs.length === 1) {
                const targetInput = pageBarcodeInputs[0];
                targetInput.focus();
                targetInput.value = code;
                targetInput.dispatchEvent(new Event('input', { bubbles: true }));
                targetInput.dispatchEvent(new Event('change', { bubbles: true }));
                targetInput.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', code: 'Enter', bubbles: true }));
                return;
            }

            // 2. If in-page-only mode, do not open global modal
            if (this.state.status === 'in_page_only') {
                if (window.showToast) window.showToast('info', `Scanned: ${code} (In-page mode only)`);
                return;
            }

            // 3. Global Action Hub Modal Fallback
            this.resolveAndShowModal(code);
        },

        resolveAndShowModal: async function(code) {
            const modal = document.getElementById('globalScanModal');
            const codeText = document.getElementById('scanModalCodeText');
            const loading = document.getElementById('scanModalLoading');
            const content = document.getElementById('scanModalContent');

            if (!modal) return;

            modal.hidden = false;
            modal.classList.add('active');
            if (codeText) codeText.textContent = code;
            if (loading) loading.style.display = 'block';
            if (content) {
                content.style.display = 'none';
                content.innerHTML = '';
            }

            try {
                const response = await fetch(`/api/scanner/resolve?code=${encodeURIComponent(code)}`);
                if (!response.ok) throw new Error('Network error resolving scanned code');
                const data = await response.json();

                if (data && data.success && data.data) {
                    this.renderModalContent(data.data);
                } else {
                    this.renderModalContent({
                        entityType: 'Unknown',
                        code: code,
                        title: 'Unknown Scanned Code',
                        subtitle: 'No details found',
                        details: { 'Raw Value': code },
                        actions: [
                            { actionId: 'catalog', label: 'Register in Catalog', icon: 'plus-circle', targetUrl: `/Catalog?newBarcode=${encodeURIComponent(code)}`, buttonClass: 'button-primary', shortcutKey: '1' },
                            { actionId: 'search', label: 'Search Catalog', icon: 'search', targetUrl: `/Catalog?search=${encodeURIComponent(code)}`, buttonClass: 'button-command', shortcutKey: '2' }
                        ]
                    });
                }
            } catch (err) {
                if (loading) loading.style.display = 'none';
                if (content) {
                    content.style.display = 'block';
                    content.innerHTML = `<div class="scan-detail-card" style="color:#ef4444;">Failed to resolve entity: ${err.message}</div>`;
                }
            }
        },

        renderModalContent: function(res) {
            this.currentModalResult = res;
            const loading = document.getElementById('scanModalLoading');
            const content = document.getElementById('scanModalContent');
            if (loading) loading.style.display = 'none';
            if (!content) return;

            const typeLower = (res.entityType || 'unknown').toLowerCase();
            const badgeClass = `scan-entity-badge ${typeLower}`;

            let detailsHtml = '';
            if (res.details) {
                for (const [key, val] of Object.entries(res.details)) {
                    detailsHtml += `
                        <div class="scan-detail-card">
                            <div class="scan-detail-key">${key}</div>
                            <div class="scan-detail-val">${val}</div>
                        </div>
                    `;
                }
            }

            let actionsHtml = '';
            if (res.actions && res.actions.length > 0) {
                actionsHtml = res.actions.map((act, idx) => `
                    <a href="${act.targetUrl}" class="scan-action-btn ${act.buttonClass || 'button-command'}" data-shortcut="${act.shortcutKey || (idx + 1)}">
                        <div class="scan-action-label-group">
                            <span>${act.label}</span>
                        </div>
                        <kbd class="scan-shortcut-kbd">${act.shortcutKey || (idx + 1)}</kbd>
                    </a>
                `).join('');
            }

            const thumbnailHtml = res.thumbnailUrl
                ? `<img src="${res.thumbnailUrl}" class="scan-entity-img" alt="${res.title}" loading="lazy" />`
                : `<div class="scan-entity-icon-ph">🏷️</div>`;

            content.innerHTML = `
                <div class="scan-entity-hero">
                    ${thumbnailHtml}
                    <div class="scan-entity-meta">
                        <span class="${badgeClass}">${res.entityType}</span>
                        <h4 class="scan-entity-title">${res.title}</h4>
                        <p class="scan-entity-sub">${res.subtitle || ''}</p>
                    </div>
                </div>

                ${detailsHtml ? `<div class="scan-details-grid">${detailsHtml}</div>` : ''}

                <div class="scan-actions-section">
                    <div class="scan-actions-title">Contextual Quick Actions</div>
                    <div class="scan-actions-grid">${actionsHtml}</div>
                </div>
            `;

            content.style.display = 'block';
        },

        setupModalEvents: function() {
            const modal = document.getElementById('globalScanModal');
            const btnClose = document.getElementById('btnCloseScanModal');
            const btnDismiss = document.getElementById('btnDismissScanModal');
            const btnCopy = document.getElementById('btnCopyScanCode');

            const closeModal = () => {
                if (modal) {
                    modal.hidden = true;
                    modal.classList.remove('active');
                }
                this.currentModalResult = null;
            };

            if (btnClose) btnClose.addEventListener('click', closeModal);
            if (btnDismiss) btnDismiss.addEventListener('click', closeModal);

            if (btnCopy) {
                btnCopy.addEventListener('click', () => {
                    const text = document.getElementById('scanModalCodeText')?.textContent;
                    if (text && text !== '---') {
                        navigator.clipboard.writeText(text);
                        if (window.showToast) window.showToast('info', 'Code copied to clipboard');
                    }
                });
            }

            // Keyboard navigation for modal (1-9 to trigger action, Esc to close)
            window.addEventListener('keydown', (e) => {
                if (!modal || modal.hidden) return;

                if (e.key === 'Escape') {
                    closeModal();
                    return;
                }

                // Check 1-9 shortcuts
                if (/^[1-9]$/.test(e.key)) {
                    const targetBtn = modal.querySelector(`[data-shortcut="${e.key}"]`);
                    if (targetBtn) {
                        e.preventDefault();
                        
                        // Delay execution slightly to ensure this isn't the first character of a rapid hardware scan
                        setTimeout(() => {
                            if (this.buffer && this.buffer.length > 1) {
                                return; // Cancel shortcut, a barcode burst is in progress
                            }
                            targetBtn.click();
                        }, 50);
                    }
                }
            });
        },

        setupFabControls: function() {
            const fabBtn = document.getElementById('btnToggleScannerFab');
            const popover = document.getElementById('scannerFabMenu');
            const resumeBtn = document.getElementById('btnResumeScanner');
            const btnRunTest = document.getElementById('btnRunTestScan');
            const testInput = document.getElementById('scannerTestInput');

            if (fabBtn && popover) {
                fabBtn.addEventListener('click', (e) => {
                    e.stopPropagation();
                    const isHidden = popover.hidden;
                    popover.hidden = !isHidden;
                    fabBtn.setAttribute('aria-expanded', isHidden ? 'true' : 'false');
                });

                document.addEventListener('click', (e) => {
                    if (!popover.contains(e.target) && !fabBtn.contains(e.target)) {
                        popover.hidden = true;
                        fabBtn.setAttribute('aria-expanded', 'false');
                    }
                });
            }

            // Menu actions
            document.querySelectorAll('.scanner-menu-btn').forEach(btn => {
                btn.addEventListener('click', () => {
                    const action = btn.dataset.action;
                    if (action === 'pause-15m') {
                        this.setStatus('paused', 15 * 60 * 1000);
                        if (window.showToast) window.showToast('warning', 'Scanner paused for 15 minutes');
                    } else if (action === 'pause-1h') {
                        this.setStatus('paused', 60 * 60 * 1000);
                        if (window.showToast) window.showToast('warning', 'Scanner paused for 1 hour');
                    } else if (action === 'toggle-session') {
                        this.setStatus('session_disabled');
                        if (window.showToast) window.showToast('warning', 'Scanner disabled for this browser session');
                    } else if (action === 'toggle-inpage') {
                        this.setStatus('in_page_only');
                        if (window.showToast) window.showToast('info', 'Scanner set to In-Page Inputs Only (No Popups)');
                    } else if (action === 'toggle-permanent') {
                        this.setStatus('permanently_disabled');
                        if (window.showToast) window.showToast('error', 'Scanner disabled permanently');
                    } else if (action === 'resume') {
                        this.setStatus('active');
                        if (window.showToast) window.showToast('success', 'Scanner resumed and listening!');
                    }

                    if (popover) popover.hidden = true;
                });
            });

            // Test scan simulator
            if (btnRunTest && testInput) {
                const runTest = () => {
                    const val = testInput.value.trim();
                    if (!val) return;
                    testInput.value = '';
                    if (popover) popover.hidden = true;
                    this.handleHardwareScan(val);
                };

                btnRunTest.addEventListener('click', runTest);
                testInput.addEventListener('keydown', (e) => {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        runTest();
                    }
                });
            }
        },

        updateFabUI: function() {
            const statusDot = document.getElementById('fabStatusDot');
            const menuDot = document.getElementById('fabMenuStatusDot');
            const modeTag = document.getElementById('fabMenuModeTag');
            const label = document.getElementById('fabLabel');
            const bannerText = document.getElementById('scannerStatusBannerText');
            const resumeBtn = document.getElementById('btnResumeScanner');

            const status = this.state.status;

            if (status === 'active') {
                if (statusDot) { statusDot.className = 'fab-status-indicator'; }
                if (menuDot) { menuDot.className = 'fab-status-dot'; }
                if (modeTag) { modeTag.textContent = 'Active'; modeTag.style.color = '#34d399'; modeTag.style.background = 'rgba(16,185,129,0.2)'; }
                if (label) { label.textContent = 'Scanner: Active'; }
                if (bannerText) { bannerText.textContent = '🟢 Background Scanner is actively listening'; }
                if (resumeBtn) { resumeBtn.style.display = 'none'; }
            } else if (status === 'paused') {
                const minsLeft = this.state.pauseUntil ? Math.ceil((this.state.pauseUntil - Date.now()) / 60000) : 0;
                if (statusDot) { statusDot.className = 'fab-status-indicator paused'; }
                if (menuDot) { menuDot.className = 'fab-status-dot paused'; }
                if (modeTag) { modeTag.textContent = `Paused (${minsLeft}m)`; modeTag.style.color = '#fbbf24'; modeTag.style.background = 'rgba(245,158,11,0.2)'; }
                if (label) { label.textContent = `Scanner: Paused (${minsLeft}m)`; }
                if (bannerText) { bannerText.textContent = `🟡 Scanner is snoozed for ${minsLeft} more minutes`; }
                if (resumeBtn) { resumeBtn.style.display = 'flex'; }
            } else if (status === 'in_page_only') {
                if (statusDot) { statusDot.className = 'fab-status-indicator'; }
                if (menuDot) { menuDot.className = 'fab-status-dot'; }
                if (modeTag) { modeTag.textContent = 'In-Page Only'; modeTag.style.color = '#38bdf8'; modeTag.style.background = 'rgba(56,189,248,0.2)'; }
                if (label) { label.textContent = 'Scanner: In-Page'; }
                if (bannerText) { bannerText.textContent = '🎯 Scanner fills active inputs only (no global popups)'; }
                if (resumeBtn) { resumeBtn.style.display = 'flex'; }
            } else {
                if (statusDot) { statusDot.className = 'fab-status-indicator disabled'; }
                if (menuDot) { menuDot.className = 'fab-status-dot disabled'; }
                if (modeTag) { modeTag.textContent = 'Disabled'; modeTag.style.color = '#94a3b8'; modeTag.style.background = 'rgba(148,163,184,0.2)'; }
                if (label) { label.textContent = 'Scanner: Off'; }
                if (bannerText) { bannerText.textContent = '⚪ Scanner is turned off'; }
                if (resumeBtn) { resumeBtn.style.display = 'flex'; }
            }
        }
    };

    // Auto-init on page load
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            window.SmartLookup.initAll();
            window.BarcodeGuard.initAll();
            window.BarcodeScannerService.init();
        });
    } else {
        window.SmartLookup.initAll();
        window.BarcodeGuard.initAll();
        window.BarcodeScannerService.init();
    }

    // --- Custom AppDialog System ---
    window.AppDialog = {
        _cleanup: function() {
            const overlay = document.getElementById('appDialogOverlay');
            if (!overlay) return;
            const input = document.getElementById('appDialogInput');
            const icon = document.getElementById('appDialogIcon');
            const cancelBtn = document.getElementById('appDialogCancel');
            const confirmBtn = document.getElementById('appDialogConfirm');

            overlay.classList.remove('show');
            overlay.setAttribute('aria-hidden', 'true');
            setTimeout(() => {
                overlay.hidden = true;
                overlay.style.display = 'none';
                if (cancelBtn) cancelBtn.onclick = null;
                if (confirmBtn) {
                    confirmBtn.onclick = null;
                    confirmBtn.className = 'dialog-btn dialog-btn-confirm';
                }
                if (input) {
                    input.style.display = 'none';
                    input.value = '';
                }
                if (icon) icon.className = 'dialog-icon';
            }, 200);
        },

        _show: function({ title, message, confirmText = 'Confirm', cancelText = 'Cancel', type = 'info', showCancel = true, isPrompt = false, defaultValue = '' }) {
            return new Promise((resolve) => {
                const overlay = document.getElementById('appDialogOverlay');
                const titleEl = document.getElementById('appDialogTitle');
                const messageEl = document.getElementById('appDialogMessage');
                const icon = document.getElementById('appDialogIcon');
                const input = document.getElementById('appDialogInput');
                const cancelBtn = document.getElementById('appDialogCancel');
                const confirmBtn = document.getElementById('appDialogConfirm');

                if (!overlay || !titleEl) {
                    console.error('AppDialog elements not found in DOM');
                    resolve(false);
                    return;
                }

                titleEl.textContent = title || 'Confirm';
                messageEl.textContent = message || '';
                
                // Icon and Theme
                icon.className = `dialog-icon ${type}`;
                confirmBtn.className = `dialog-btn dialog-btn-confirm ${type === 'danger' ? 'danger' : ''}`;
                
                let iconSvg = '';
                if (type === 'danger') iconSvg = '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path><line x1="12" y1="9" x2="12" y2="13"></line><line x1="12" y1="17" x2="12.01" y2="17"></line></svg>';
                else if (type === 'warning') iconSvg = '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="8" x2="12" y2="12"></line><line x1="12" y1="16" x2="12.01" y2="16"></line></svg>';
                else if (type === 'success') iconSvg = '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>';
                else iconSvg = '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="16" x2="12" y2="12"></line><line x1="12" y1="8" x2="12.01" y2="8"></line></svg>';
                icon.innerHTML = iconSvg;

                // Buttons
                confirmBtn.textContent = confirmText;
                cancelBtn.textContent = cancelText;
                cancelBtn.style.display = showCancel ? 'inline-block' : 'none';

                // Prompt Input
                if (isPrompt) {
                    input.style.display = 'block';
                    input.value = defaultValue;
                } else {
                    input.style.display = 'none';
                }

                // Event Listeners
                cancelBtn.onclick = () => {
                    this._cleanup();
                    resolve(isPrompt ? null : false);
                };

                confirmBtn.onclick = () => {
                    this._cleanup();
                    resolve(isPrompt ? input.value : true);
                };

                // Show
                overlay.hidden = false;
                overlay.style.display = 'flex';
                requestAnimationFrame(() => {
                    overlay.setAttribute('aria-hidden', 'false');
                    overlay.classList.add('show');
                    if (isPrompt) input.focus();
                    else confirmBtn.focus();
                });
            });
        },



        confirm: function(options) {
            if (typeof options === 'string') options = { message: options };
            return this._show({ ...options, showCancel: true, isPrompt: false });
        },

        alert: function(options) {
            if (typeof options === 'string') options = { message: options };
            return this._show({ ...options, showCancel: false, isPrompt: false, type: options.type || 'warning', confirmText: options.confirmText || 'OK' });
        },

        prompt: function(options) {
            if (typeof options === 'string') options = { message: options };
            return this._show({ ...options, showCancel: true, isPrompt: true });
        }
    };

    // Global Interceptor for data-confirm
    document.addEventListener('click', (e) => {
        const confirmTarget = e.target.closest('[data-confirm]');
        if (confirmTarget) {
            e.preventDefault();
            e.stopPropagation();
            
            const message = confirmTarget.getAttribute('data-confirm');
            const title = confirmTarget.getAttribute('data-confirm-title') || 'Confirm';
            const type = confirmTarget.getAttribute('data-confirm-type') || 'warning';
            
            window.AppDialog.confirm({ title, message, type }).then(confirmed => {
                if (confirmed) {
                    // Temporarily remove data-confirm so we don't loop
                    const oldConfirm = confirmTarget.getAttribute('data-confirm');
                    confirmTarget.removeAttribute('data-confirm');
                    
                    if (confirmTarget.form) {
                        // If it's a submit button, append its value to the form before submitting
                        if (confirmTarget.name) {
                            const hidden = document.createElement('input');
                            hidden.type = 'hidden';
                            hidden.name = confirmTarget.name;
                            hidden.value = confirmTarget.value || '';
                            confirmTarget.form.appendChild(hidden);
                        }
                        
                        // Check if the form has an onsubmit handler that we need to bypass or trigger
                        // The safest way to submit the form is calling requestSubmit if available
                        if (confirmTarget.form.requestSubmit) {
                            confirmTarget.form.requestSubmit(confirmTarget);
                        } else {
                            confirmTarget.form.submit();
                        }
                    } else if (confirmTarget.tagName === 'A' && confirmTarget.href) {
                        window.location.href = confirmTarget.href;
                    } else {
                        // Otherwise, just click it
                        confirmTarget.click();
                    }
                    
                    // Restore attribute
                    setTimeout(() => confirmTarget.setAttribute('data-confirm', oldConfirm), 50);
                }
            });
        }
    }, true); // Use capture phase to intercept before inline onclicks run

    // --- Resilient Polling Utility with Exponential Backoff & Jitter ---
    window.pollWithExponentialBackoff = async function(pollFn, options = {}) {
        const maxAttempts = options.maxAttempts || 5;
        const initialDelayMs = options.initialDelayMs || 1500;
        const maxDelayMs = options.maxDelayMs || 12000;
        const factor = options.factor || 1.8;

        let delay = initialDelayMs;
        for (let attempt = 1; attempt <= maxAttempts; attempt++) {
            try {
                const result = await pollFn(attempt);
                if (result && result.done) {
                    return result;
                }
                if (attempt === maxAttempts) {
                    return result || { done: false, timeout: true };
                }
            } catch (err) {
                if (attempt === maxAttempts) {
                    throw err;
                }
            }

            // Jitter +/- 20%
            const jitter = delay * (0.8 + Math.random() * 0.4);
            if (typeof options.onRetry === 'function') {
                options.onRetry(attempt, Math.round(jitter));
            }
            await new Promise(res => setTimeout(res, jitter));
            delay = Math.min(maxDelayMs, delay * factor);
        }
    };

    // --- Global Form Submit Button Loading State Handler ---
    document.addEventListener('submit', (e) => {
        const form = e.target;
        if (!form || form.hasAttribute('data-no-loading')) return;

        const submitBtn = form.querySelector('button[type="submit"]:not([disabled]), input[type="submit"]:not([disabled]), button.btn_save:not([disabled]), button.button-primary:not([disabled])');
        if (submitBtn && !submitBtn.classList.contains('no-spin')) {
            submitBtn.classList.add('is-loading');
            submitBtn.setAttribute('aria-busy', 'true');
        }
    });

})();


