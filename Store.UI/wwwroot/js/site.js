(() => {
    // Modal open/close via data attributes
    const openTiles = document.querySelectorAll('[data-open-modal]');
    const closeButtons = document.querySelectorAll('[data-close-modal]');
    const allModals = document.querySelectorAll('.modalView');

    const closeAll = () => {
        allModals.forEach(m => m.classList.remove('show'));
    };

    openTiles.forEach(tile => {
        tile.addEventListener('click', e => {
            e.preventDefault();
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

    // --- Toasts ---
    window.showToast = (type, message) => {
        const container = document.getElementById('toast-container');
        if (!container) return;

        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        
        const icon = type === 'success' ? '✓' : '!';
        
        toast.innerHTML = `
            <div class="toast-icon">${icon}</div>
            <div class="toast-message">${message}</div>
            <button class="toast-close" aria-label="Close">&times;</button>
        `;

        container.appendChild(toast);

        requestAnimationFrame(() => {
            toast.classList.add('show');
        });

        const dismiss = () => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        };

        toast.querySelector('.toast-close').addEventListener('click', dismiss);
        setTimeout(dismiss, 5000);
    };

    // --- Blades ---
    window.openBlade = (id) => {
        const blade = document.getElementById(id);
        if (!blade) return;
        const overlayId = blade.getAttribute('data-overlay-id');
        if (overlayId) {
            const overlay = document.getElementById(overlayId);
            if (overlay) overlay.classList.add('open');
        }
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
        blade.classList.remove('open');
    };

    document.querySelectorAll('[data-close-blade]').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const id = btn.getAttribute('data-close-blade');
            window.closeBlade(id);
        });
    });

    document.querySelectorAll('.blade-overlay').forEach(overlay => {
        overlay.addEventListener('click', () => {
            const bladeId = overlay.getAttribute('data-blade-id');
            if (bladeId) window.closeBlade(bladeId);
        });
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

})();
