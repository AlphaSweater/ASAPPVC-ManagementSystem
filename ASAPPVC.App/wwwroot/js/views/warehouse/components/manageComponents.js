/**
 * Live search functionality for Components Management page
 * Now requests server-rendered rows partial instead of building HTML in JS.
 */

(function () {
	'use strict';

	// Configuration
	const DEBOUNCE_DELAY = 300; // ms to wait after user stops typing
	const MIN_SEARCH_LENGTH = 0; // minimum characters to trigger search (0 = search on empty too)

	let debounceTimer = null;
	let currentRequest = null;

	/**
	* Initialize the live search functionality
	*/
	function init() {
		const searchInput = document.querySelector('.search-form input[name="term"]');
		const searchForm = document.querySelector('.search-form');

		if (!searchInput || !searchForm) {
			console.warn('Search elements not found on page');
			return;
		}

		// Prevent default form submission
		searchForm.addEventListener('submit', (e) => {
			e.preventDefault();
			performSearch(searchInput.value.trim());
		});

		// Live search on input
		searchInput.addEventListener('input', (e) => {
			const term = e.target.value.trim();

			// Clear existing debounce timer
			if (debounceTimer) {
				clearTimeout(debounceTimer);
			}

			// Debounce the search
			debounceTimer = setTimeout(() => {
				if (term.length >= MIN_SEARCH_LENGTH) {
					performSearch(term);
				}
			}, DEBOUNCE_DELAY);
		});

		// Clear search with Escape
		searchInput.addEventListener('keydown', (e) => {
			if (e.key === 'Escape') {
				searchInput.value = '';
				performSearch('');
			}
		});
	}

	/**
	* Perform the search via AJAX requesting server-rendered rows partial
	* @param {string} term - Search term
	*/
	function performSearch(term) {
		// Cancel any pending request
		if (currentRequest) {
			currentRequest.abort();
		}

		const controller = new AbortController();
		currentRequest = controller;
		const url = `/Warehouse/Components/Search?term=${encodeURIComponent(term)}`;

		// Show loading state
		setLoadingState(true);

		fetch(url, {
			method: 'GET',
			headers: {
				'Accept': 'text/html, application/json'
			},
			signal: controller.signal
		})
			.then(async response => {
				if (!response.ok) {
					throw new Error(`HTTP error! status: ${response.status}`);
				}
				const contentType = response.headers.get('content-type') || '';
				if (contentType.indexOf('application/json') !== -1) {
					return { json: await response.json() };
				}
				return { html: await response.text() };
			})
			.then(result => {
				if (result.json) {
					// Controller returns JSON for errors (success:false)
					const data = result.json;
					if (!data || data.success === false) {
						// show error as table text instead of popup
						showError(data?.error || 'Failed to search components');
						return;
					}
					// If for some reason success=true JSON is returned, do nothing (we expect HTML for rows)
				}
				else if (result.html !== undefined) {
					const tbody = document.querySelector('.table-components tbody');
					if (!tbody) return;
					tbody.innerHTML = result.html;
					// Re-initialize copy buttons and any other client-side enhancers
					if (window.initCopyButtons) {
						window.initCopyButtons();
					}
				}
			})
			.catch(error => {
				if (error.name !== 'AbortError') {
					console.error('Search error:', error);
					// Show error inside the table rather than a modal popup
					showError('An error occurred while searching. Please try again.');
				}
			})
			.finally(() => {
				setLoadingState(false);
				currentRequest = null;
			});
	}

	/**
	* Set loading state on the table
	* @param {boolean} isLoading - Whether loading is active
	*/
	function setLoadingState(isLoading) {
		const table = document.querySelector('.table-components');
		if (table) {
			if (isLoading) {
				table.classList.add('is-loading');
				table.style.opacity = '0.6';
				table.style.pointerEvents = 'none';
			} else {
				table.classList.remove('is-loading');
				table.style.opacity = '';
				table.style.pointerEvents = '';
			}
		}
	}

	/**
	* Show error message inside the table as fallback
	* @param {string} message - Error message to display
	*/
	function showError(message) {
		const tbody = document.querySelector('.table-components tbody');
		if (!tbody) return;
		tbody.innerHTML = `
<tr>
 <td colspan="7" class="ft-center" role="alert" style="padding:40px; color:var(--error, #dc3545);">
 ${escapeHtml(message)}
 </td>
</tr>
`;
	}

	/**
	* Escape HTML to prevent XSS
	* @param {string} unsafe - Unsafe string
	* @returns {string} Escaped string
	*/
	function escapeHtml(unsafe) {
		if (!unsafe) return '';
		return unsafe
			.toString()
			.replace(/&/g, "&amp;")
			.replace(/</g, "&lt;")
			.replace(/>/g, "&gt;")
			.replace(/"/g, "&quot;")
			.replace(/'/g, "&#039;");
	}

	// Initialize on DOM ready
	if (document.readyState === 'loading') {
		document.addEventListener('DOMContentLoaded', init);
	} else {
		init();
	}
})();