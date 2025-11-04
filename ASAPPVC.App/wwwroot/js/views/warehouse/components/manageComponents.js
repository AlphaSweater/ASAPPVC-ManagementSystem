/**
 * Live search functionality for Components Management page
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
	* Perform the search via AJAX
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
				'Accept': 'application/json'
			},
			signal: controller.signal
		})
			.then(response => {
				if (!response.ok) {
					throw new Error(`HTTP error! status: ${response.status}`);
				}
				return response.json();
			})
			.then(data => {
				if (data.success) {
					renderResults(data.components, term);
				} else {
					showError(data.error || 'Failed to search components');
				}
			})
			.catch(error => {
				if (error.name !== 'AbortError') {
					console.error('Search error:', error);
					showError('An error occurred while searching. Please try again.');
				}
			})
			.finally(() => {
				setLoadingState(false);
				currentRequest = null;
			});
	}

	/**
	* Render search results
	* @param {Array} components - Array of component objects
	* @param {string} searchTerm - The search term used
	*/
	function renderResults(components, searchTerm) {
		const tbody = document.querySelector('.table-components tbody');
		if (!tbody) return;

		if (!components || components.length === 0) {
			tbody.innerHTML = `
<tr>
 <td colspan="7" class="ft-center" role="status" style="padding:40px; color:var(--text-muted);">
 ${searchTerm ? `No components found for "${escapeHtml(searchTerm)}".` : 'No components found.'}
 </td>
</tr>
`;
			return;
		}

		tbody.innerHTML = components.map(component => createComponentRow(component)).join('');

		// Re-initialize copy buttons if they exist
		if (window.initCopyButtons) {
			window.initCopyButtons();
		}
	}

	/**
	* Create HTML for a component table row
	* @param {Object} component - Component object
	* @returns {string} HTML string
	*/
	function createComponentRow(component) {
		const hasImage = component.hasImage && component.thumbUrl;
		const imageHtml = hasImage
			? `<img src="${escapeHtml(component.thumbUrl)}" class="ft-media__img" loading="lazy" alt="" width="52" height="52" />`
			: `<div class="ft-media__placeholder" aria-hidden="true">🧩</div>`;

		return `
<tr>
 <td class="ft-media">
 ${imageHtml}
 </td>
 <td class="ft-code" title="${escapeHtml(component.componentCode)}">
 <div class="ft-code__text">
 <span>${escapeHtml(component.componentCode)}</span>
 <button type="button" class="ft-code__copy" data-copy="${escapeHtml(component.componentCode)}" data-action="copy" aria-label="Copy component code">
 <lucideIcon class="icon" name="copy" size="16" stroke-width="1.75" />
 </button>
 </div>
 </td>
 <td class="ft-name" title="${escapeHtml(component.componentName)}">
 <span class="ft-clip ft-clip--2">${escapeHtml(component.componentName)}</span>
 </td>
 <td class="ft-location">${escapeHtml(component.locationCode)}</td>
 <td class="ft-stock">${escapeHtml(component.shortFormattedQuantity)}</td>
 <td class="ft-money">${escapeHtml(component.displayCost)}</td>
 <td class="ft-actions">
 <div class="actions">
 ${component.componentCode ?
				`<a class="btn btn-secondary btn-sm" href="/Warehouse/Components/View/${encodeURIComponent(component.componentCode)}" aria-label="View ${escapeHtml(component.componentName)}">View</a>
 <a class="btn btn-primary btn-sm" href="/Warehouse/Components/Edit/${encodeURIComponent(component.componentCode)}" aria-label="Edit ${escapeHtml(component.componentName)}">Edit</a>`
				:
				`<a class="btn btn-secondary btn-sm" href="/Warehouse/Components/View/${encodeURIComponent(component.id)}" aria-label="View ${escapeHtml(component.componentName)}">View</a>
 <a class="btn btn-primary btn-sm" href="/Warehouse/Components/Edit/${encodeURIComponent(component.id)}" aria-label="Edit ${escapeHtml(component.componentName)}">Edit</a>`
			}
 </div>
 </td>
</tr>
`;
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
	* Show error message
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