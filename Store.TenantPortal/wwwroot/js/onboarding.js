// Onboarding Wizard Management, URL Indexing & Debounced Slug Validation
let currentStep = 1;
let slugDebounceTimer = null;
let isSlugValid = false;

document.addEventListener('DOMContentLoaded', () => {
    initSlugChecker();
    initPlanSelectors();
    initDomainSelectors();
    initUrlStepIndexing();
});

// URL Step Indexing & Browser Navigation
function initUrlStepIndexing() {
    const urlParams = new URLSearchParams(window.location.search);
    const stepParam = parseInt(urlParams.get('step') || '1', 10);

    if (stepParam >= 1 && stepParam <= 4) {
        goToStep(stepParam, false);
    } else {
        // Initialize state
        history.replaceState({ step: 1 }, '', '?step=1');
    }

    window.addEventListener('popstate', (e) => {
        if (e.state && e.state.step) {
            goToStep(e.state.step, false);
        } else {
            const params = new URLSearchParams(window.location.search);
            const step = parseInt(params.get('step') || '1', 10);
            goToStep(step, false);
        }
    });
}

function initSlugChecker() {
    const slugInput = document.getElementById('StoreSlug');
    const slugBadge = document.getElementById('slugCheckBadge');

    if (!slugInput || !slugBadge) return;

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
                slugBadge.innerHTML = '<span style="color: #f87171; font-size: 12px;">Error checking availability</span>';
                isSlugValid = false;
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
        }
    });

    radioCustom.addEventListener('change', () => {
        if (radioCustom.checked) {
            customGroup.style.display = 'block';
            document.getElementById('DomainChoice').value = 'Custom';
        }
    });
}

function goToStep(step, updateHistory = true) {
    if (step > currentStep) {
        if (!validateStep(currentStep)) return;
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
    }

    if (step === 4) {
        populateSummary();
    }
}

function validateStep(step) {
    if (step === 1) {
        const u = document.getElementById('AdminUsername')?.value.trim();
        const p = document.getElementById('AdminPassword')?.value;
        const cp = document.getElementById('ConfirmAdminPassword')?.value;

        if (!u || !p) {
            alert('Please fill out admin username and password.');
            return false;
        }
        if (p.length < 8) {
            alert('Password must be at least 8 characters.');
            return false;
        }
        if (p !== cp) {
            alert('Admin passwords do not match.');
            return false;
        }
        return true;
    }

    if (step === 2) {
        const name = document.getElementById('StoreName')?.value.trim();
        const slug = document.getElementById('StoreSlug')?.value.trim();

        if (!name || !slug) {
            alert('Please enter a store name and slug.');
            return false;
        }
        if (!isSlugValid) {
            alert('Please choose a valid and available store slug.');
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

    if (summaryName) summaryName.innerText = storeName;
    if (summarySlug) summarySlug.innerText = storeSlug;
    if (summaryAdmin) summaryAdmin.innerText = adminUser;
    
    if (summaryUrl) {
        if (domainChoice === 'Custom' && customDomain) {
            summaryUrl.innerText = `https://${customDomain}`;
        } else {
            summaryUrl.innerText = `https://${storeSlug}.store.clexan.com`;
        }
    }
}
