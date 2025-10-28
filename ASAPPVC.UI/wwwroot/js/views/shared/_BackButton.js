(() => {
	const backLinks = document.querySelectorAll('.back-button-link');

	backLinks.forEach(link => {
		const useHistory = link.getAttribute('data-use-history') === 'true';
		const showPageName = link.getAttribute('data-show-page-name') === 'true';
		const pageNameSpan = link.querySelector('.back-page-name');

		// Get referrer page name from document.referrer
		if (showPageName && pageNameSpan) {
			const referrer = document.referrer;
			if (referrer) {
				try {
					const url = new URL(referrer);
					const pathParts = url.pathname.split('/').filter(p => p);
					const pageName = pathParts[pathParts.length - 1] || 'previous page';

					// Capitalize and format the name
					const formattedName = pageName
						.replace(/([A-Z])/g, ' $1')
						.replace(/[-_]/g, ' ')
						.trim()
						.split(' ')
						.map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
						.join(' ');

					pageNameSpan.textContent = ` to ${formattedName}`;
				} catch (e) {
					pageNameSpan.textContent = '';
				}
			} else {
				pageNameSpan.textContent = '';
			}
		}

		// Handle click for history navigation
		if (useHistory) {
			link.addEventListener('click', (e) => {
				e.preventDefault();

				// Check if there's history to go back to
				if (window.history.length > 1) {
					window.history.back();
				} else {
					// Fallback to home or a default route
					window.location.href = '/';
				}
			});
		}
	});
})();