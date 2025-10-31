(() => {
	'use strict';

	const opts = (window && window.__UpsertComponentOptions) || {};
	const intHandlers = new WeakMap();

	function enableIntegerMode(el) {
		if (!el || intHandlers.has(el)) return;
		el.setAttribute('inputmode', 'numeric');
		el.setAttribute('pattern', '[0-9]*');
		el.step = '1';

		const onKeyDown = e => {
			// Allow navigation and control keys
			if (
				["Backspace", "Tab", "ArrowLeft", "ArrowRight", "Delete", "Enter", "Home", "End"].includes(e.key) ||
				e.ctrlKey || e.metaKey
			) return;

			// Prevent decimal separators, exponent, signs
			if (e.key === '.' || e.key === ',' || e.key === 'e' || e.key === 'E' || e.key === '+' || e.key === '-') {
				e.preventDefault();
			}
		};

		const onBlur = () => {
			const v = Number(el.value);
			if (!Number.isNaN(v)) el.value = Math.round(v).toString();
		};

		const onPaste = e => {
			e.preventDefault();
			const text = (e.clipboardData || window.clipboardData).getData('text') || '';
			const digits = text.replace(/\D+/g, '');
			el.value = digits ? String(Number(digits)) : '';
		};

		el.addEventListener('keydown', onKeyDown);
		el.addEventListener('blur', onBlur);
		el.addEventListener('paste', onPaste);
		intHandlers.set(el, { onKeyDown, onBlur, onPaste });
	}

	function disableIntegerMode(el) {
		if (!el) return;
		el.removeAttribute('inputmode');
		el.removeAttribute('pattern');
		// restore default step if not set elsewhere
		el.step = '0.01';
		const handlers = intHandlers.get(el);
		if (handlers) {
			el.removeEventListener('keydown', handlers.onKeyDown);
			el.removeEventListener('blur', handlers.onBlur);
			el.removeEventListener('paste', handlers.onPaste);
			intHandlers.delete(el);
		}
	}

	function setNumericSteps(isInteger) {
		const qty = document.querySelector('.qty-input');
		const reorder = document.querySelector('input[name="ReorderLevel"]') || document.getElementById('ReorderLevel');
		if (qty) {
			qty.step = isInteger ? '1' : '0.01';
			if (isInteger) {
				enableIntegerMode(qty);
				// If switching to integer, round existing value
				if (qty.value) {
					const v = Number(qty.value);
					if (!Number.isNaN(v)) qty.value = Math.round(v).toString();
				}
			} else {
				disableIntegerMode(qty);
			}
		}
		if (reorder) {
			reorder.step = isInteger ? '1' : '0.01';
			if (isInteger) {
				enableIntegerMode(reorder);
				if (reorder.value) {
					const v = Number(reorder.value);
					if (!Number.isNaN(v)) reorder.value = Math.round(v).toString();
				}
			} else {
				disableIntegerMode(reorder);
			}
		}
	}

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

		// New: adjust step attributes for qty/reorder based on unit
		const unitSelect = document.querySelector('.unit-select');
		const integerOnly = Array.isArray(opts.integerOnlyUnitValues) ? opts.integerOnlyUnitValues.map(Number) : [0, 1];
		function applyStepForCurrentUnit() {
			if (!unitSelect) return;
			const val = Number(unitSelect.value);
			const isInt = integerOnly.includes(val);
			setNumericSteps(isInt);
		}

		if (unitSelect) {
			unitSelect.addEventListener('change', applyStepForCurrentUnit);
			// initialize on load
			applyStepForCurrentUnit();
		} else {
			// default to decimal steps
			setNumericSteps(false);
		}
	}

	if (document.readyState === 'loading') {
		document.addEventListener('DOMContentLoaded', init);
	} else {
		init();
	}
})();