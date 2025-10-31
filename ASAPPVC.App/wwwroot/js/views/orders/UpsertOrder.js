(function () {
	'use strict';

	// Read JSON payload injected into the page
	function readPageData() {
		try {
			const el = document.getElementById('upsert-order-data');
			if (!el) return { available: [], selected: [] };
			return JSON.parse(el.textContent || el.innerText || '{}');
		} catch (e) {
			console.error('Failed to parse upsert order data', e);
			return { available: [], selected: [] };
		}
	}

	const pageData = readPageData();
	const available = Array.isArray(pageData.available) ? pageData.available : [];
	const selected = Array.isArray(pageData.selected) ? pageData.selected : [];

	// Products dynamic table elements
	const tbody = document.getElementById('productsTbody');
	const grandEl = document.getElementById('grandTotal');
	const addBtn = document.getElementById('addProductBtn');
	const searchInput = document.getElementById('productSearch');
	const quantityInput = document.getElementById('quantityInput');
	const dataList = document.getElementById('productsList');

	// Build lookup map from available products
	const byId = new Map();
	for (const p of available) {
		const id = p.Id ?? p.id ?? p.ProductId;
		const name = p.ProductName ?? p.Name ?? p.name ?? 'Unnamed Product';
		const code = p.ProductCode ?? p.Code ?? p.code ?? '';
		const price = Number(p.SellingPrice ?? p.Price ?? p.price ?? 0) || 0;
		const label = code ? `${code} — ${name}` : name;
		if (id) byId.set(String(id), { id: String(id), name, code, label, price });
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
				el.name = el.name.replace(/Products\[\d+\]/g, `Products[${i}]`);
			});
		});
	}

	function rowExistsFor(productId) {
		if (!tbody) return null;
		return tbody.querySelector(`tr[data-id="${productId}"]`);
	}

	function updateTotals() {
		if (!tbody || !grandEl) return;
		let grand = 0;
		Array.from(tbody.querySelectorAll('tr')).forEach(tr => {
			const id = tr.getAttribute('data-id');
			const qty = parseInt(tr.querySelector('.qty-input')?.value || '0', 10) || 0;
			const unitPriceCell = tr.querySelector('.unit-price');
			const subCell = tr.querySelector('.subtotal');

			const product = byId.get(String(id));
			const unitPrice = product ? Number(product.price || 0) : 0;
			const subtotal = unitPrice * qty;

			if (unitPriceCell) unitPriceCell.textContent = formatMoney(unitPrice);
			if (subCell) subCell.textContent = formatMoney(subtotal);
			grand += subtotal;
		});
		grandEl.textContent = formatMoney(grand);
	}

	function createRow(product, quantity = 1) {
		if (!tbody) return;
		const tr = document.createElement('tr');
		tr.setAttribute('data-id', product.id);

		tr.innerHTML = `
			<td class="ft-grow">
				<div class="cell-stack">
					<div class="cell-title">${product.label}</div>
					${product.code ? `<div class="cell-sub">Code: ${product.code}</div>` : ''}
				</div>
				<input type="hidden" name="Products[9999].ProductId" value="${product.id}" />
			</td>

			<td class="ft-center">
				<input class="input qty-input" type="number"
					name="Products[9999].Quantity"
					value="${quantity}" min="1" step="1" />
			</td>

			<td class="ft-money unit-price">R0.00</td>
			<td class="ft-money subtotal">R0.00</td>

			<td class="ft-center">
				<button type="button" class="btn btn-remove" title="Remove">
					<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
						<path fill-rule="evenodd" d="M9 2a1 1 0 0 0-.894.553L7.382 4H4a1 1 0 0 0 0 2v10a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2V6a1 1 0 1 0 0-2h-3.382l-.724-1.447A1 1 0 0 0 11 2H9zM7 8a1 1 0 0 1 2 0v6a1 1 0 1 1-2 0V8zm4 0a1 1 0 0 1 2 0v6a1 1 0 1 1-2 0V8z" clip-rule="evenodd"/>
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

	// Add product by typed text (matches datalist option)
	function addFromSearchBox() {
		if (!searchInput || !quantityInput) return;
		const typed = (searchInput.value || '').trim();
		if (!typed) return;

		const qty = parseInt(quantityInput.value || '1', 10) || 1;

		let productId = null;
		if (dataList) {
			for (const opt of dataList.options) {
				if (opt.value === typed && opt.dataset.id) {
					productId = opt.dataset.id;
					break;
				}
			}
		}

		if (!productId) {
			for (const [id, p] of byId.entries()) {
				if (p.label === typed || p.name === typed || p.code === typed) {
					productId = id;
					break;
				}
			}
		}

		if (!productId || !byId.has(String(productId))) return;

		const existing = rowExistsFor(productId);
		if (existing) {
			const qtyInput = existing.querySelector('.qty-input');
			const current = parseInt(qtyInput.value || '0', 10) || 0;
			qtyInput.value = (current + qty).toString();
			updateTotals();
		} else {
			createRow(byId.get(String(productId)), qty);
		}

		searchInput.value = '';
		quantityInput.value = '1';
		searchInput.focus();
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
			const id = String(s.ProductId ?? s.productId ?? s.id ?? '');
			const qty = parseInt(s.Quantity ?? s.quantity ?? s.OrderedQuantity ?? s.qty ?? 1, 10) || 1;

			if (!byId.has(id)) {
				// If product not in available list, create a placeholder entry
				byId.set(id, {
					id,
					name: 'Unknown product',
					code: '',
					label: 'Unknown product',
					price: 0
				});
			}
			createRow(byId.get(id), qty);
		}
	}
})();