(function () {
	const ACTION_ATTR = 'data-action';
	const COPY_ATTR = 'data-copy';
	const DEFAULT_TIMEOUT = 1500;

	function showTooltip(text, x, y) {
		const t = document.createElement('div');
		t.className = 'app-copy-tooltip';
		t.textContent = text;
		Object.assign(t.style, {
			position: 'fixed',
			top: (y - 36) + 'px',
			left: (x) + 'px',
			transform: 'translateX(-50%)',
			background: '#111',
			color: '#fff',
			padding: '6px8px',
			borderRadius: '4px',
			fontSize: '12px',
			zIndex: 10000,
			pointerEvents: 'none',
			opacity: '0',
			transition: 'opacity150ms ease, transform150ms ease'
		});

		document.body.appendChild(t);
		// Trigger transition
		requestAnimationFrame(() => {
			t.style.opacity = '1';
			t.style.transform = 'translateX(-50%) translateY(-6px)';
		});

		setTimeout(() => {
			t.style.opacity = '0';
			t.style.transform = 'translateX(-50%) translateY(-12px)';
			setTimeout(() => {
				if (t.parentNode) t.parentNode.removeChild(t);
			}, 180);
		}, DEFAULT_TIMEOUT);
	}

	async function handleAction(elem, event) {
		const action = elem.getAttribute(ACTION_ATTR) || elem.dataset.action || 'copy';

		if (action === 'copy') {
			const value = elem.getAttribute(COPY_ATTR) ?? elem.dataset.copy ?? '';
			if (!value) return;

			try {
				if (navigator.clipboard && navigator.clipboard.writeText) {
					await navigator.clipboard.writeText(value);
				} else {
					// Fallback for older browsers
					const ta = document.createElement('textarea');
					ta.value = value;
					ta.style.position = 'fixed';
					ta.style.left = '-9999px';
					document.body.appendChild(ta);
					ta.select();
					document.execCommand('copy');
					document.body.removeChild(ta);
				}

				const x = event.clientX || (elem.getBoundingClientRect().left + elem.offsetWidth / 2);
				const y = event.clientY || (elem.getBoundingClientRect().top);
				showTooltip('Copied to clipboard!', x, y);

				// Emit a custom event so other scripts can react
				elem.dispatchEvent(new CustomEvent('copied', { detail: { value } }));
			} catch (err) {
				console.error('Copy failed', err);
				showTooltip('Failed to copy', event.clientX || 0, event.clientY || 0);
			}
		} else {
			// Placeholder for future actions. Emit a generic event.
			elem.dispatchEvent(new CustomEvent('action', { detail: { action, elem } }));
		}
	}

	function onClick(e) {
		const btn = e.target.closest('.ft-code__copy, [data-action]');
		if (!btn) return;
		handleAction(btn, e);
	}

	// Initialize
	document.addEventListener('click', onClick);

	// Public API for other scripts
	window.AppCopy = {
		copy: async (text) => {
			if (!text) return Promise.reject(new Error('No text provided'));
			if (navigator.clipboard && navigator.clipboard.writeText) return navigator.clipboard.writeText(text);
			return new Promise((resolve, reject) => {
				try {
					const ta = document.createElement('textarea');
					ta.value = text;
					ta.style.position = 'fixed';
					ta.style.left = '-9999px';
					document.body.appendChild(ta);
					ta.select();
					document.execCommand('copy');
					document.body.removeChild(ta);
					resolve();
				} catch (e) { reject(e); }
			});
		}
	};
})();