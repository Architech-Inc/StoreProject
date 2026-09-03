/**
 * Reusable International Phone Input Widget with Country Code Selector
 */
(function () {
    const DEFAULT_COUNTRIES = [
        { iso: 'CM', name: 'Cameroon', dial: '+237', flag: '🇨🇲' },
        { iso: 'NG', name: 'Nigeria', dial: '+234', flag: '🇳🇬' },
        { iso: 'GA', name: 'Gabon', dial: '+241', flag: '🇬🇦' },
        { iso: 'CG', name: 'Congo', dial: '+242', flag: '🇨🇬' },
        { iso: 'CD', name: 'DR Congo', dial: '+243', flag: '🇨🇩' },
        { iso: 'TD', name: 'Chad', dial: '+235', flag: '🇹🇩' },
        { iso: 'CF', name: 'Central African Republic', dial: '+236', flag: '🇨🇫' },
        { iso: 'GQ', name: 'Equatorial Guinea', dial: '+240', flag: '🇬🇶' },
        { iso: 'CI', name: 'Ivory Coast', dial: '+225', flag: '🇨🇮' },
        { iso: 'GH', name: 'Ghana', dial: '+233', flag: '🇬🇭' },
        { iso: 'SN', name: 'Senegal', dial: '+221', flag: '🇸🇳' },
        { iso: 'ML', name: 'Mali', dial: '+223', flag: '🇲🇱' },
        { iso: 'BF', name: 'Burkina Faso', dial: '+226', flag: '🇧🇫' },
        { iso: 'BJ', name: 'Benin', dial: '+229', flag: '🇧🇯' },
        { iso: 'TG', name: 'Togo', dial: '+228', flag: '🇹🇬' },
        { iso: 'NE', name: 'Niger', dial: '+227', flag: '🇳🇪' },
        { iso: 'KE', name: 'Kenya', dial: '+254', flag: '🇰🇪' },
        { iso: 'RW', name: 'Rwanda', dial: '+250', flag: '🇷🇼' },
        { iso: 'UG', name: 'Uganda', dial: '+256', flag: '🇺🇬' },
        { iso: 'TZ', name: 'Tanzania', dial: '+255', flag: '🇹🇿' },
        { iso: 'ZA', name: 'South Africa', dial: '+27', flag: '🇿🇦' },
        { iso: 'ET', name: 'Ethiopia', dial: '+251', flag: '🇪🇹' },
        { iso: 'EG', name: 'Egypt', dial: '+20', flag: '🇪🇬' },
        { iso: 'MA', name: 'Morocco', dial: '+212', flag: '🇲🇦' },
        { iso: 'DZ', name: 'Algeria', dial: '+213', flag: '🇩🇿' },
        { iso: 'TN', name: 'Tunisia', dial: '+216', flag: '🇹🇳' },
        { iso: 'FR', name: 'France', dial: '+33', flag: '🇫🇷' },
        { iso: 'GB', name: 'United Kingdom', dial: '+44', flag: '🇬🇧' },
        { iso: 'US', name: 'United States', dial: '+1', flag: '🇺🇸' },
        { iso: 'CA', name: 'Canada', dial: '+1', flag: '🇨🇦' },
        { iso: 'DE', name: 'Germany', dial: '+49', flag: '🇩🇪' },
        { iso: 'BE', name: 'Belgium', dial: '+32', flag: '🇧🇪' },
        { iso: 'CH', name: 'Switzerland', dial: '+41', flag: '🇨🇭' },
        { iso: 'ES', name: 'Spain', dial: '+34', flag: '🇪🇸' },
        { iso: 'IT', name: 'Italy', dial: '+39', flag: '🇮🇹' },
        { iso: 'NL', name: 'Netherlands', dial: '+31', flag: '🇳🇱' },
        { iso: 'PT', name: 'Portugal', dial: '+351', flag: '🇵🇹' },
        { iso: 'TR', name: 'Turkey', dial: '+90', flag: '🇹🇷' },
        { iso: 'AE', name: 'United Arab Emirates', dial: '+971', flag: '🇦🇪' },
        { iso: 'SA', name: 'Saudi Arabia', dial: '+966', flag: '🇸🇦' },
        { iso: 'CN', name: 'China', dial: '+86', flag: '🇨🇳' },
        { iso: 'IN', name: 'India', dial: '+91', flag: '🇮🇳' },
        { iso: 'BR', name: 'Brazil', dial: '+55', flag: '🇧🇷' }
    ];

    let countriesList = DEFAULT_COUNTRIES;

    // Async fetch live country database if available
    fetch('/api/countries')
        .then(r => r.ok ? r.json() : null)
        .then(res => {
            if (res && res.data && Array.isArray(res.data) && res.data.length > 0) {
                countriesList = res.data.map(c => ({
                    iso: c.isoCode || '',
                    name: c.name,
                    dial: c.phoneCode || '+237',
                    flag: c.flagEmoji || '🌐'
                }));
            }
        })
        .catch(() => { /* fallback to DEFAULT_COUNTRIES */ });

    function findCountryByDial(dial) {
        if (!dial) return countriesList[0];
        const clean = dial.startsWith('+') ? dial : '+' + dial;
        return countriesList.find(c => c.dial === clean) || countriesList[0];
    }

    function findCountryByIso(iso) {
        if (!iso) return countriesList[0];
        const clean = iso.trim().toUpperCase();
        return countriesList.find(c => c.iso === clean) || countriesList[0];
    }

    function parseRawValue(value, defaultDial = '+237') {
        if (!value) return { country: findCountryByDial(defaultDial), nationalNumber: '' };
        let clean = value.replace(/[^\d+]/g, '');
        if (clean.startsWith('00')) clean = '+' + clean.substring(2);

        if (clean.startsWith('+')) {
            // Sort by longest dial code
            const sorted = [...countriesList].sort((a, b) => b.dial.length - a.dial.length);
            for (const c of sorted) {
                if (clean.startsWith(c.dial)) {
                    return { country: c, nationalNumber: clean.substring(c.dial.length) };
                }
            }
            // If unknown + code, default to fallback
            return { country: findCountryByDial(defaultDial), nationalNumber: clean.replace(/^\+/, '') };
        } else {
            // Check if user entered numbers starting with a known dial without plus (e.g. 237678787878)
            const sorted = [...countriesList].sort((a, b) => b.dial.length - a.dial.length);
            for (const c of sorted) {
                const codeNoPlus = c.dial.replace('+', '');
                if (clean.startsWith(codeNoPlus) && clean.length > codeNoPlus.length + 5) {
                    return { country: c, nationalNumber: clean.substring(codeNoPlus.length) };
                }
            }
            return { country: findCountryByDial(defaultDial), nationalNumber: clean.replace(/^0+/, '') };
        }
    }

    function initWidget(el) {
        if (el.dataset.phoneInitialized === 'true') return;
        el.dataset.phoneInitialized = 'true';

        // Target hidden or underlying target input
        const targetInput = el.tagName === 'INPUT' ? el : el.querySelector('input[type="tel"], input[name*="Phone"]');
        if (!targetInput) return;

        const defaultIso = targetInput.dataset.defaultCountry || 'CM';
        const defaultDial = targetInput.dataset.defaultDial || '+237';
        const parsed = parseRawValue(targetInput.value, defaultDial);
        let currentCountry = parsed.country || findCountryByIso(defaultIso);

        // Hide original input if we are building a wrapper around it
        const wrap = document.createElement('div');
        wrap.className = 'phone-input-wrap';
        targetInput.parentNode.insertBefore(wrap, targetInput);

        // Move targetInput inside or make it hidden
        const isTargetVisible = targetInput.getAttribute('type') !== 'hidden';
        if (isTargetVisible) {
            targetInput.type = 'hidden';
        }
        wrap.appendChild(targetInput);

        // Visible elements
        const countryBtn = document.createElement('button');
        countryBtn.type = 'button';
        countryBtn.className = 'phone-country-btn';
        countryBtn.innerHTML = `
            <span class="phone-country-flag">${currentCountry.flag}</span>
            <span class="phone-country-dial">${currentCountry.dial}</span>
            <span class="phone-country-chevron">▼</span>
        `;
        wrap.appendChild(countryBtn);

        const nationalInput = document.createElement('input');
        nationalInput.type = 'tel';
        nationalInput.className = 'phone-number-field';
        nationalInput.placeholder = targetInput.placeholder || '6 00 00 00 00';
        nationalInput.value = formatNationalDisplay(parsed.nationalNumber, currentCountry.iso);
        wrap.appendChild(nationalInput);

        // Dropdown popover
        const dropdown = document.createElement('div');
        dropdown.className = 'phone-country-dropdown';
        dropdown.innerHTML = `
            <div class="phone-country-search-wrap">
                <input type="text" class="phone-country-search" placeholder="Search country or code..." />
            </div>
            <ul class="phone-country-list"></ul>
        `;
        wrap.appendChild(dropdown);

        const searchInput = dropdown.querySelector('.phone-country-search');
        const listEl = dropdown.querySelector('.phone-country-list');

        function renderList(query = '') {
            listEl.innerHTML = '';
            const q = query.trim().toLowerCase();
            const filtered = countriesList.filter(c => 
                !q || 
                c.name.toLowerCase().includes(q) || 
                c.dial.toLowerCase().includes(q) || 
                c.iso.toLowerCase().includes(q)
            );

            if (filtered.length === 0) {
                listEl.innerHTML = '<li style="padding:10px 12px; font-size:0.8rem; color:var(--text-tertiary);">No countries found</li>';
                return;
            }

            filtered.forEach(c => {
                const li = document.createElement('li');
                li.className = 'phone-country-item' + (c.iso === currentCountry.iso ? ' active' : '');
                li.innerHTML = `
                    <div class="phone-country-item-left">
                        <span>${c.flag}</span>
                        <span>${c.name}</span>
                    </div>
                    <span class="phone-country-item-dial">${c.dial}</span>
                `;
                li.addEventListener('click', (e) => {
                    e.stopPropagation();
                    selectCountry(c);
                    closeDropdown();
                });
                listEl.appendChild(li);
            });
        }

        function selectCountry(c) {
            currentCountry = c;
            countryBtn.querySelector('.phone-country-flag').textContent = c.flag;
            countryBtn.querySelector('.phone-country-dial').textContent = c.dial;
            syncTarget();
        }

        function syncTarget() {
            const rawDigits = nationalInput.value.replace(/\D/g, '');
            if (rawDigits.length > 0) {
                targetInput.value = `${currentCountry.dial}${rawDigits}`;
            } else {
                targetInput.value = '';
            }
            // Trigger change event on targetInput
            targetInput.dispatchEvent(new Event('input', { bubbles: true }));
            targetInput.dispatchEvent(new Event('change', { bubbles: true }));
        }

        function openDropdown() {
            dropdown.classList.add('open');
            searchInput.value = '';
            renderList();
            setTimeout(() => searchInput.focus(), 50);
        }

        function closeDropdown() {
            dropdown.classList.remove('open');
        }

        countryBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            if (dropdown.classList.contains('open')) {
                closeDropdown();
            } else {
                // Close any other open dropdowns
                document.querySelectorAll('.phone-country-dropdown.open').forEach(d => d.classList.remove('open'));
                openDropdown();
            }
        });

        searchInput.addEventListener('input', (e) => {
            renderList(e.target.value);
        });

        nationalInput.addEventListener('input', (e) => {
            const val = e.target.value;
            // Auto detect international prefix typed or pasted
            if (val.startsWith('+') || val.startsWith('00')) {
                const detected = parseRawValue(val, currentCountry.dial);
                if (detected.country && detected.country.iso !== currentCountry.iso) {
                    selectCountry(detected.country);
                }
                nationalInput.value = formatNationalDisplay(detected.nationalNumber, currentCountry.iso);
            } else {
                // Format digits
                const digits = val.replace(/\D/g, '');
                nationalInput.value = formatNationalDisplay(digits, currentCountry.iso);
            }
            syncTarget();
        });

        // Close when clicking outside
        document.addEventListener('click', (e) => {
            if (!wrap.contains(e.target)) {
                closeDropdown();
            }
        });

        // Allow programmatically setting value
        wrap.setPhone = function(val) {
            const parsed = parseRawValue(val, currentCountry.dial);
            if (parsed.country) selectCountry(parsed.country);
            nationalInput.value = formatNationalDisplay(parsed.nationalNumber, currentCountry.iso);
            syncTarget();
        };

        targetInput._phoneWrap = wrap;
    }

    function formatNationalDisplay(digits, iso) {
        if (!digits) return '';
        if (iso === 'CM' && digits.length <= 9) {
            // 6 78 78 78 78
            if (digits.length <= 1) return digits;
            let res = digits[0] + ' ';
            for (let i = 1; i < digits.length; i += 2) {
                res += digits.substring(i, Math.min(i + 2, digits.length)) + ' ';
            }
            return res.trim();
        }
        // General grouping by 3s or 2s
        if (digits.length >= 7) {
            let res = '';
            for (let i = 0; i < digits.length; i += 3) {
                res += digits.substring(i, Math.min(i + 3, digits.length)) + ' ';
            }
            return res.trim();
        }
        return digits;
    }

    // Public API
    window.PhoneInput = {
        initAll: function (root = document) {
            root.querySelectorAll('[data-phone-input], .phone-input-control').forEach(initWidget);
        },
        init: initWidget,
        setValue: function (inputOrSelector, val) {
            const input = typeof inputOrSelector === 'string' ? document.querySelector(inputOrSelector) : inputOrSelector;
            if (input && input._phoneWrap && input._phoneWrap.setPhone) {
                input._phoneWrap.setPhone(val);
            } else if (input) {
                input.value = val;
            }
        }
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => window.PhoneInput.initAll());
    } else {
        window.PhoneInput.initAll();
    }
})();
