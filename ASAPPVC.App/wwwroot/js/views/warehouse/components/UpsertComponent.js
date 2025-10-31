(() => {
	'use strict';

	const opts = (window && window.__UpsertComponentOptions) || {};

	function init() {
		const input = document.getElementById('imageFile');
		const preview = document.getElementById('previewImg');
		const zone = document.getElementById('dropzone');

		const previewFile = file => {
			if (!file || !file.type || !file.type.startsWith('image/')) return;
			const reader = new FileReader();
			reader.onload = e => { preview.src = e.target.result; };
			reader.readAsDataURL(file);
		};

		if (input) {
			input.addEventListener('change', () => previewFile(input.files[0]));
		}

		['dragenter', 'dragover'].forEach(evt =>
			zone && zone.addEventListener(evt, e => { e.preventDefault(); zone.classList.add('is-dragover'); })
		);

		['dragleave', 'drop'].forEach(evt =>
			zone && zone.addEventListener(evt, e => { e.preventDefault(); zone.classList.remove('is-dragover'); })
		);

		if (zone) {
			zone.addEventListener('drop', e => {
				const file = e.dataTransfer && e.dataTransfer.files && e.dataTransfer.files[0];
				if (file && input) { input.files = e.dataTransfer.files; previewFile(file); }
			});

			zone.addEventListener('keydown', e => {
				if (['Enter', ' '].includes(e.key)) { e.preventDefault(); input && input.click(); }
			});
		}

		// Header status toggle sync -> Status <select>
		const headerCheckbox = document.getElementById('IsActiveHeader');
		const headerLabel = document.getElementById('IsActiveLabel');
		const headerText = document.getElementById('IsActiveText');
		if (headerCheckbox && headerLabel && headerText) {
			const setState = () => {
				headerLabel.setAttribute('aria-pressed', headerCheckbox.checked ? 'true' : 'false');
				headerLabel.classList.toggle('btn-primary', headerCheckbox.checked);
				headerLabel.classList.toggle('btn-secondary', !headerCheckbox.checked);
				headerText.textContent = headerCheckbox.checked ? 'Active' : 'Inactive';
				const selectId = opts.isActiveSelectId;
				if (selectId) {
					const select = document.getElementById(selectId);
					if (select && select.tagName === 'SELECT') {
						select.value = headerCheckbox.checked ? 'true' : 'false';
					}
				}
			};
			setState();
			headerCheckbox.addEventListener('change', setState);
		}
	}

	if (document.readyState === 'loading') {
		document.addEventListener('DOMContentLoaded', init);
	} else {
		init();
	}
})();