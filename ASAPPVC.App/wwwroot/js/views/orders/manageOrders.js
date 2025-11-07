(function () {
    'use strict';

    const DEBOUNCE_DELAY = 300;
    const MIN_SEARCH_LENGTH = 0;

    let debounceTimer = null;
    let currentRequest = null;

    function init() {
        const form = document.querySelector('.search-form');
        const termInput = form?.querySelector('input[name="term"]');
        const statusSelect = form?.querySelector('select[name="status"]');

        if (!form || !termInput || !statusSelect) {
            console.warn('Orders search elements not found');
            return;
        }

        // 🔹 On page load, set default status to "pendingpicked" if not already
        if (!statusSelect.value || statusSelect.value === 'all') {
            statusSelect.value = 'pendingpicked';
        }

        // 🔹 Automatically run the first search (default Pending + Picked)
        performSearch(termInput.value.trim(), statusSelect.value);

        // 🔹 Handle manual search submit
        form.addEventListener('submit', (e) => {
            e.preventDefault();
            performSearch(termInput.value.trim(), statusSelect.value);
        });

        // 🔹 Handle typing search (debounced)
        termInput.addEventListener('input', () => {
            const term = termInput.value.trim();
            if (debounceTimer) clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                if (term.length >= MIN_SEARCH_LENGTH) {
                    performSearch(term, statusSelect.value);
                }
            }, DEBOUNCE_DELAY);
        });

        // 🔹 Handle status filter change
        statusSelect.addEventListener('change', () => {
            performSearch(termInput.value.trim(), statusSelect.value);
        });

        // 🔹 Handle ESC key to clear search
        termInput.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                termInput.value = '';
                performSearch('', statusSelect.value);
            }
        });
    }

    function performSearch(term, status) {
        if (currentRequest) currentRequest.abort();
        const controller = new AbortController();
        currentRequest = controller;

        const params = new URLSearchParams();
        params.set('term', term || '');
        params.set('status', status || 'pendingpicked'); // 🔹 default fallback

        const url = `/Orders/Search?${params.toString()}`;
        setLoading(true);

        fetch(url, {
            method: 'GET',
            headers: { 'Accept': 'text/html, application/json' },
            signal: controller.signal
        })
            .then(async (res) => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                const ct = res.headers.get('content-type') || '';
                if (ct.includes('application/json')) return { json: await res.json() };
                return { html: await res.text() };
            })
            .then((result) => {
                const tbody = document.querySelector('.table-orders tbody');
                if (!tbody) return;

                if (result.json) {
                    if (!result.json || result.json.success === false) {
                        tbody.innerHTML = errorRow(result.json?.error || 'Failed to search orders.');
                    }
                } else if (result.html !== undefined) {
                    tbody.innerHTML = result.html;
                }
            })
            .catch((err) => {
                if (err.name !== 'AbortError') {
                    console.error('Orders search error:', err);
                    const tbody = document.querySelector('.table-orders tbody');
                    if (tbody) tbody.innerHTML = errorRow('An error occurred while searching. Please try again.');
                }
            })
            .finally(() => {
                setLoading(false);
                currentRequest = null;
            });
    }

    function setLoading(isLoading) {
        const table = document.querySelector('.table-orders');
        if (!table) return;
        table.classList.toggle('is-loading', !!isLoading);
        table.style.opacity = isLoading ? '0.6' : '';
        table.style.pointerEvents = isLoading ? 'none' : '';
    }

    function errorRow(message) {
        const safe = escapeHtml(message);
        return `
<tr>
  <td colspan="6" class="ft-center" role="alert" style="padding:40px; color:var(--error, #dc3545);">
    ${safe}
  </td>
</tr>`;
    }

    function escapeHtml(unsafe) {
        if (!unsafe) return '';
        return unsafe.toString()
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
