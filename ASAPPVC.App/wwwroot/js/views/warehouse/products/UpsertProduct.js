(function () {
	'use strict';

	// Read JSON payload injected into the page
	function readPageData() {
		try {
			const el = document.getElementById('upsert-product-data');
			if (!el) return { available: [], selected: [] };
			return JSON.parse(el.textContent || el.innerText || '{}');
		} catch (e) {
			console.error('Failed to parse upsert product data', e);
			return { available: [], selected: [] };
		}
	}

	const pageData = readPageData();
	const available = Array.isArray(pageData.available) ? pageData.available : [];
	const selected = Array.isArray(pageData.selected) ? pageData.selected : [];

	// Image preview
	const fileInput = document.getElementById('imageFile');
	const preview = document.getElementById('previewImg');
	if (fileInput && preview) {
		fileInput.addEventListener('change', function () {
			const f = this.files && this.files[0];
			if (!f) return;
			const reader = new FileReader();
			reader.onload = e => { preview.src = e.target.result; };
			reader.readAsDataURL(f);
		});
		const imageBox = document.querySelector('.image-box');
		if (imageBox) imageBox.addEventListener('click', () => fileInput.click());
	}

	// Parts dynamic table elements
	const tbody = document.getElementById('partsTbody');
	const grandEl = document.getElementById('grandTotal');
	const addBtn = document.getElementById('addComponentBtn');
	const searchInput = document.getElementById('componentSearch');
	const dataList = document.getElementById('componentsList');

	// Build lookup map from available components
	const byId = new Map();
	for (const c of available) {
		const id = c.ComponentId ?? c.componentId ?? c.id;
		const name = c.ComponentName ?? c.Name ?? c.name ?? 'Unnamed';
		const code = c.ComponentCode ?? c.Code ?? c.code;
		const unit = c.UnitOfMeasure ?? c.Unit ?? c.unit ?? '';
		const price = Number(c.UnitCost ?? c.unitCost ?? c.Price ?? 0) || 0;
		const label = code ? `${code} — ${name}` : name;
		if (id) byId.set(String(id), { id: String(id), name, code, label, unit, price });
	}

	// Format money (ZAR)
	function formatMoney(n) {
		const val = Number(n || 0);
		try {
			return new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR', maximumFractionDigits: 2 }).format(val);
		} catch {
			return `R${val.toFixed(2)}`;
		}
	}

	// Re-number input names for model binder
	function renumberIndexes() {
		if (!tbody) return;
		Array.from(tbody.querySelectorAll('tr')).forEach((tr, i) => {
			tr.querySelectorAll('[name]').forEach(el => {
				el.name = el.name.replace(/SelectedProductComponents\[\d+\]/g, `SelectedProductComponents[${i}]`);
			});
		});
	}

	function rowExistsFor(componentId) {
		if (!tbody) return null;
		return tbody.querySelector(`tr[data-id="${componentId}"]`);
	}

	function updateTotals() {
		if (!tbody || !grandEl) return;
		let grand = 0;
		Array.from(tbody.querySelectorAll('tr')).forEach(tr => {
			const id = tr.getAttribute('data-id');
			const qty = parseFloat(tr.querySelector('.qty-input')?.value || '0') || 0;
			const unitPriceCell = tr.querySelector('.unit-price');
			const subCell = tr.querySelector('.subtotal');

			const comp = byId.get(String(id));
			const unitPrice = comp ? Number(comp.price || 0) : 0;
			const subtotal = unitPrice * qty;

			if (unitPriceCell) unitPriceCell.textContent = formatMoney(unitPrice);
			if (subCell) subCell.textContent = formatMoney(subtotal);
			grand += subtotal;
		});
		grandEl.textContent = formatMoney(grand);
	}

	function createRow(comp, quantity = 1) {
		if (!tbody) return;
		const tr = document.createElement('tr');
		tr.setAttribute('data-id', comp.id);

		tr.innerHTML = `
			 <td>
			 <div class="cell-stack">
			 <div class="cell-title">${comp.label}</div>
			 <div class="cell-sub">${comp.unit ? `Unit: ${comp.unit}` : ''}</div>
			 </div>
			 <input type="hidden" name="SelectedProductComponents[9999].ComponentId" value="${comp.id}" />
			 </td>

			 <td style="text-align:center;">
			 <input class="input qty-input" type="number"
			 name="SelectedProductComponents[9999].RequiredQuantity"
			 value="${quantity}" min="0.01" step="0.01" />
			 </td>

			 <td class="align-right unit-price">R0.00</td>
			 <td class="align-right subtotal">R0.00</td>

			 <td style="text-align:center;">
			 <button type="button" class="btn btn-remove" title="Remove">
			 <svg xmlns="http://www.w3.org/2000/svg" viewBox="002020" fill="currentColor" width="18" height="18">
			 <path fill-rule="evenodd" d="M92a11000-.894.553L7.3824H4a1100002v10a2200022h8a220002-2V6a110100-2h-3.382l-.724-1.447A11000112H9zM78a1100120v6a11011-20V8zm40a1100120v6a11011-20V8z" clip-rule="evenodd"/>
			 </svg>
			 </button>
			 </td>`;

		const qtyInput = tr.querySelector('.qty-input');
		const removeBtn = tr.querySelector('.btn-remove');

		qtyInput?.addEventListener('input', () => { updateTotals(); });
		removeBtn?.addEventListener('click', () => {
			tr.remove();
			renumberIndexes();
			updateTotals();
		});

		tbody.appendChild(tr);
		renumberIndexes();
		updateTotals();
	}

	// Add component by typed text (matches datalist option)
	function addFromSearchBox() {
		if (!searchInput) return;
		const typed = (searchInput.value || '').trim();
		if (!typed) return;

		let componentId = null;
		if (dataList) {
			for (const opt of dataList.options) {
				if (opt.value === typed && opt.dataset.id) {
					componentId = opt.dataset.id;
					break;
				}
			}
		}

		if (!componentId) {
			for (const [id, c] of byId.entries()) {
				if (c.label === typed || c.name === typed || c.code === typed) {
					componentId = id;
					break;
				}
			}
		}

		if (!componentId || !byId.has(String(componentId))) return;

		const existing = rowExistsFor(componentId);
		if (existing) {
			const qtyInput = existing.querySelector('.qty-input');
			const current = parseFloat(qtyInput.value || '0') || 0;
			qtyInput.value = (current + 1).toString();
			updateTotals();
		} else {
			createRow(byId.get(String(componentId)), 1);
		}

		searchInput.value = '';
	}

	// Wire up add button and Enter key
	addBtn?.addEventListener('click', addFromSearchBox);
	searchInput?.addEventListener('keydown', (e) => {
		if (e.key === 'Enter') {
			e.preventDefault();
			addFromSearchBox();
		}
	});

	// Initialize from selected (edit mode)
	if (Array.isArray(selected) && selected.length > 0) {
		for (const s of selected) {
			const id = String(s.ComponentId ?? s.componentId ?? s.id ?? '');
			const qty = Number(s.RequiredQuantity ?? s.requiredQuantity ?? s.qty ?? 1) || 1;

			if (!byId.has(id)) {
				byId.set(id, {
					id,
					name: 'Unknown component',
					code: '',
					label: 'Unknown component',
					unit: '',
					price: 0
				});
			}
			createRow(byId.get(id), qty);
		}
	}
})();