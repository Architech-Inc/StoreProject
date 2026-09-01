// Onboarding Wizard Management, Session Draft Persistence, URL Indexing & Debounced Slug Validation
let currentStep = 1;
let slugDebounceTimer = null;
let isSlugValid = true;

const STORAGE_KEY = 'clexan_onboarding_draft_v1';

document.addEventListener('DOMContentLoaded', () => {
    restoreDraftFromStorage();
    initSlugChecker();
    initPlanSelectors();
    initDomainSelectors();
    initAutoSave();
    initUrlStepIndexing();
    initFormSubmission();
});

// Auto-Save Draft to SessionStorage
function initAutoSave() {
    const inputs = document.querySelectorAll('#onboardingForm input, #onboardingForm select');
    inputs.forEach(input => {
        input.addEventListener('input', saveDraftToStorage);
        input.addEventListener('change', saveDraftToStorage);
    });
}

function saveDraftToStorage() {
    try {
        const draft = {
            adminUsername: document.getElementById('AdminUsername')?.value || '',
            adminPassword: document.getElementById('AdminPassword')?.value || '',
            confirmAdminPassword: document.getElementById('ConfirmAdminPassword')?.value || '',
            storeName: document.getElementById('StoreName')?.value || '',
            storeSlug: document.getElementById('StoreSlug')?.value || '',
            currency: document.getElementById('Input_Currency')?.value || document.querySelector('select[name="Input.Currency"]')?.value || 'XAF',
            planTier: document.getElementById('PlanTier')?.value || '1',
            domainChoice: document.getElementById('DomainChoice')?.value || 'Platform',
            customDomain: document.getElementById('CustomDomain')?.value || ''
        };
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify(draft));
    } catch (e) {
        console.warn('Draft save error:', e);
    }
}

function restoreDraftFromStorage() {
    try {
        const raw = sessionStorage.getItem(STORAGE_KEY);
        if (!raw) return;
        const draft = JSON.parse(raw);

        const setVal = (id, val) => {
            const el = document.getElementById(id);
            if (el && val) el.value = val;
        };

        setVal('AdminUsername', draft.adminUsername);
        setVal('AdminPassword', draft.adminPassword);
        setVal('ConfirmAdminPassword', draft.confirmAdminPassword);
        setVal('StoreName', draft.storeName);
        setVal('StoreSlug', draft.storeSlug);
        setVal('PlanTier', draft.planTier);
        setVal('DomainChoice', draft.domainChoice);
        setVal('CustomDomain', draft.customDomain);

        const currEl = document.querySelector('select[name="Input.Currency"]');
        if (currEl && draft.currency) currEl.value = draft.currency;

        // Restore Plan UI
        if (draft.planTier) {
            document.querySelectorAll('.plan-card').forEach(c => {
                c.classList.toggle('selected', c.getAttribute('data-tier') === draft.planTier);
            });
        }

        // Restore Domain UI
        if (draft.domainChoice === 'Custom') {
            const rCustom = document.getElementById('domainCustom');
            const customGroup = document.getElementById('customDomainGroup');
            if (rCustom) rCustom.checked = true;
            if (customGroup) customGroup.style.display = 'block';
        }
    } catch (e) {
        console.warn('Draft restore error:', e);
    }
}

// URL Step Indexing & Browser Navigation
function initUrlStepIndexing() {
    const urlParams = new URLSearchParams(window.location.search);
    const stepParam = parseInt(urlParams.get('step') || '1', 10);

    // Calculate maximum reachable step based on filled fields
    const maxReachable = getMaxReachableStep();
    const targetStep = Math.min(Math.max(1, stepParam), maxReachable);

    goToStep(targetStep, false, false);

    window.addEventListener('popstate', (e) => {
        if (e.state && e.state.step) {
            goToStep(e.state.step, false, false);
        } else {
            const params = new URLSearchParams(window.location.search);
            const step = parseInt(params.get('step') || '1', 10);
            goToStep(Math.min(step, getMaxReachableStep()), false, false);
        }
    });
}

function getMaxReachableStep() {
    const u = document.getElementById('AdminUsername')?.value?.trim();
    const p = document.getElementById('AdminPassword')?.value;
    if (!u || !p || p.length < 8) return 1;

    const name = document.getElementById('StoreName')?.value?.trim();
    const slug = document.getElementById('StoreSlug')?.value?.trim();
    if (!name || !slug || slug.length < 3) return 2;

    return 4;
}

function initSlugChecker() {
    const slugInput = document.getElementById('StoreSlug');
    const slugBadge = document.getElementById('slugCheckBadge');

    if (!slugInput || !slugBadge) return;

    // Check on restore
    if (slugInput.value.trim().length >= 3) {
        slugBadge.innerHTML = '<span style="color: #4ade80; font-size: 12px; font-weight: 600;">✓ Ready</span>';
        isSlugValid = true;
    }

    slugInput.addEventListener('input', () => {
        const rawSlug = slugInput.value.trim().toLowerCase().replace(/[^a-z0-9-]/g, '-');
        slugInput.value = rawSlug;

        if (rawSlug.length < 3) {
            slugBadge.innerHTML = '<span style="color: var(--p-text-subtle); font-size: 12px;">(Min 3 chars)</span>';
            isSlugValid = false;
            return;
        }

        slugBadge.innerHTML = '<span style="color: #fbbf24; font-size: 12px;">Checking availability...</span>';

        clearTimeout(slugDebounceTimer);
        slugDebounceTimer = setTimeout(async () => {
            try {
                const response = await fetch(`/api/slugs/check?slug=${encodeURIComponent(rawSlug)}`);
                if (!response.ok) {
                    throw new Error(`HTTP error ${response.status}`);
                }
                const result = await response.json();

                if (result.isAvailable) {
                    slugBadge.innerHTML = '<span style="color: #4ade80; font-size: 12px; font-weight: 600;">✓ Available</span>';
                    isSlugValid = true;
                } else {
                    slugBadge.innerHTML = `<span style="color: #f87171; font-size: 12px; font-weight: 600;">✗ ${result.reason || 'Unavailable'}</span>`;
                    isSlugValid = false;
                }
            } catch (err) {
                slugBadge.innerHTML = '<span style="color: #4ade80; font-size: 12px;">✓ Ready</span>';
                isSlugValid = true;
            }
        }, 300);
    });
}

function initPlanSelectors() {
    document.querySelectorAll('.plan-card').forEach(card => {
        card.addEventListener('click', () => {
            document.querySelectorAll('.plan-card').forEach(c => c.classList.remove('selected'));
            card.classList.add('selected');
            const tierVal = card.getAttribute('data-tier');
            const input = document.getElementById('PlanTier');
            if (input) input.value = tierVal;
            saveDraftToStorage();
        });
    });
}

function initDomainSelectors() {
    const radioPlatform = document.getElementById('domainPlatform');
    const radioCustom = document.getElementById('domainCustom');
    const customGroup = document.getElementById('customDomainGroup');

    if (!radioPlatform || !radioCustom || !customGroup) return;

    radioPlatform.addEventListener('change', () => {
        if (radioPlatform.checked) {
            customGroup.style.display = 'none';
            document.getElementById('DomainChoice').value = 'Platform';
            saveDraftToStorage();
        }
    });

    radioCustom.addEventListener('change', () => {
        if (radioCustom.checked) {
            customGroup.style.display = 'block';
            document.getElementById('DomainChoice').value = 'Custom';
            saveDraftToStorage();
        }
    });
}

function goToStep(step, updateHistory = true, showAlert = true) {
    if (step > currentStep) {
        if (!validateStep(currentStep, showAlert)) return;
    }

    document.querySelectorAll('.wizard-step-panel').forEach(panel => panel.style.display = 'none');
    const targetPanel = document.getElementById(`stepPanel${step}`);
    if (targetPanel) {
        targetPanel.style.display = 'block';
    }

    document.querySelectorAll('.step-node').forEach(node => {
        const s = parseInt(node.getAttribute('data-step'), 10);
        node.classList.remove('active', 'completed');
        if (s === step) {
            node.classList.add('active');
        } else if (s < step) {
            node.classList.add('completed');
        }
    });

    currentStep = step;

    if (updateHistory) {
        history.pushState({ step }, '', `?step=${step}`);
    } else {
        history.replaceState({ step }, '', `?step=${step}`);
    }

    if (step === 4) {
        populateSummary();
    }
}

function validateStep(step, showAlert = true) {
    if (step === 1) {
        const u = document.getElementById('AdminUsername')?.value.trim();
        const p = document.getElementById('AdminPassword')?.value;
        const cp = document.getElementById('ConfirmAdminPassword')?.value;

        if (!u || !p) {
            if (showAlert) ClexAn.alert('Please fill out admin username and password.', 'Credentials Required', 'warning');
            return false;
        }
        if (p.length < 8) {
            if (showAlert) ClexAn.alert('Password must be at least 8 characters.', 'Password Too Short', 'warning');
            return false;
        }
        if (p !== cp) {
            if (showAlert) ClexAn.alert('Admin passwords do not match.', 'Password Mismatch', 'error');
            return false;
        }
        return true;
    }

    if (step === 2) {
        const name = document.getElementById('StoreName')?.value.trim();
        const slug = document.getElementById('StoreSlug')?.value.trim();

        if (!name || !slug) {
            if (showAlert) ClexAn.alert('Please enter a store name and slug.', 'Store Identity Required', 'warning');
            return false;
        }
        if (slug.length < 3) {
            if (showAlert) ClexAn.alert('Store slug must be at least 3 characters.', 'Invalid Slug Length', 'warning');
            return false;
        }
        if (!isSlugValid) {
            if (showAlert) ClexAn.alert('Please choose a valid and available store slug.', 'Slug Unavailable', 'warning');
            return false;
        }
        return true;
    }

    return true;
}

function populateSummary() {
    const storeName = document.getElementById('StoreName')?.value;
    const storeSlug = document.getElementById('StoreSlug')?.value;
    const adminUser = document.getElementById('AdminUsername')?.value;
    const domainChoice = document.getElementById('DomainChoice')?.value;
    const customDomain = document.getElementById('CustomDomain')?.value;

    const summaryName = document.getElementById('summaryStoreName');
    const summarySlug = document.getElementById('summaryStoreSlug');
    const summaryAdmin = document.getElementById('summaryAdminUser');
    const summaryUrl = document.getElementById('summaryStoreUrl');

    if (summaryName) summaryName.innerText = storeName || '—';
    if (summarySlug) summarySlug.innerText = storeSlug || '—';
    if (summaryAdmin) summaryAdmin.innerText = adminUser || '—';
    
    if (summaryUrl) {
        if (domainChoice === 'Custom' && customDomain) {
            summaryUrl.innerText = `https://${customDomain}`;
        } else if (storeSlug) {
            summaryUrl.innerText = `https://${storeSlug}.store.clexan.com`;
        } else {
            summaryUrl.innerText = '—';
        }
    }
}

function initFormSubmission() {
    const form = document.getElementById('onboardingForm');
    const launchBtn = document.getElementById('btnLaunch');
    if (!form || !launchBtn) return;

    form.addEventListener('submit', (e) => {
        // Ensure all steps are valid
        if (!validateStep(1, true)) {
            e.preventDefault();
            goToStep(1);
            return;
        }
        if (!validateStep(2, true)) {
            e.preventDefault();
            goToStep(2);
            return;
        }

        // Show provisioning UI state
        launchBtn.disabled = true;
        launchBtn.innerHTML = `
            <svg class="animate-spin" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="display: inline-block; vertical-align: middle; margin-right: 8px;">
                <circle cx="12" cy="12" r="10" stroke-opacity="0.25"></circle>
                <path d="M12 2a10 10 0 0 1 10 10" stroke-linecap="round"></path>
            </svg>
            Provisioning Silo Containers...
        `;

        // Clear draft on successful submission
        sessionStorage.removeItem(STORAGE_KEY);
    });
}
