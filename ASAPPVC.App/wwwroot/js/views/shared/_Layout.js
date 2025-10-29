(function () {
	// --------- Accordion with auto-height and smooth animations ----------
	document.querySelectorAll('.sb-group').forEach(group => {
		const toggle = group.querySelector('.sb-toggle');
		const panel = group.querySelector('.sb-subnav');
		if (!toggle || !panel) return;

		function setHeight(open) {
			if (open) {
				// Set to actual content height
				panel.style.maxHeight = (panel.scrollHeight + 20) + 'px'; // +20 for padding
			} else {
				panel.style.maxHeight = '0px';
			}
		}

		// Initialize from server-provided aria-expanded
		const initiallyOpen = group.getAttribute('aria-expanded') === 'true';
		setHeight(initiallyOpen);
		toggle.setAttribute('aria-expanded', initiallyOpen ? 'true' : 'false');

		// Navigation click handling - no manual toggle, just navigate
		toggle.addEventListener('click', (e) => {
			// Let the link navigation happen naturally
			// The page reload will set the correct aria-expanded state
		});

		// Recalculate height on resize if open
		let resizeTimer;
		window.addEventListener('resize', () => {
			clearTimeout(resizeTimer);
			resizeTimer = setTimeout(() => {
				if (group.getAttribute('aria-expanded') === 'true') {
					// Temporarily disable transition for instant resize
					panel.style.transition = 'none';
					setHeight(true);
					// Re-enable transition after a frame
					requestAnimationFrame(() => {
						panel.style.transition = '';
					});
				}
			}, 100);
		});
	});

	// --------- Mobile burger & overlay toggle ----------
	const body = document.body;
	const burger = document.querySelector('.sb-burger');
	const overlay = document.querySelector('.sb-overlay');
	const sidebar = document.getElementById('sidebar');

	function setSidebarOpen(open) {
		body.classList.toggle('sidebar--open', open);
		if (overlay) overlay.hidden = !open;
		if (burger) burger.setAttribute('aria-expanded', String(open));
	}

	if (burger && overlay && sidebar) {
		burger.addEventListener('click', (e) => {
			e.stopPropagation();
			const open = !body.classList.contains('sidebar--open');
			setSidebarOpen(open);
		});

		overlay.addEventListener('click', () => setSidebarOpen(false));

		// Close on Escape key
		document.addEventListener('keydown', (e) => {
			if (e.key === 'Escape' && body.classList.contains('sidebar--open')) {
				setSidebarOpen(false);
			}
		});

		// Close sidebar when clicking a link on mobile
		if (window.innerWidth <= 900) {
			sidebar.querySelectorAll('a').forEach(link => {
				link.addEventListener('click', () => {
					setSidebarOpen(false);
				});
			});
		}
	}

	// --------- Smooth scroll to top on navigation (optional polish) ----------
	document.querySelectorAll('.sidebar a').forEach(link => {
		link.addEventListener('click', () => {
			// Add a subtle active animation
			link.style.transform = 'scale(0.98)';
			setTimeout(() => {
				link.style.transform = '';
			}, 150);
		});
	});
})();