// JavaScript for Add Order page
document.addEventListener('DOMContentLoaded', function () {
	const addProductBtn = document.getElementById('add-product-btn');
	const productSelect = document.getElementById('product-select');
	const quantityInput = document.getElementById('quantity-input');
	const orderItemsContainer = document.getElementById('order-items-container');
	const hiddenInputsContainer = document.getElementById('order-items-hidden-inputs');

	const orderItems = {};

	if (addProductBtn) addProductBtn.addEventListener('click', handleAddProduct);

	if (orderItemsContainer) orderItemsContainer.addEventListener('click', function (e) {
		if (e.target && e.target.classList.contains('btn-remove')) {
			const row = e.target.closest('tr');
			if (!row) return;
			const productId = row.dataset.productId;
			handleRemoveProduct(productId);
		}
	});

	function handleAddProduct() {
		if (!productSelect) return;
		const selectedOption = productSelect.options[productSelect.selectedIndex];
		if (!selectedOption || !selectedOption.value) {
			alert('Please select a product.');
			return;
		}

		const productId = selectedOption.value;
		const quantity = parseInt(quantityInput ? quantityInput.value : '0', 10);

		if (isNaN(quantity) || quantity < 1) {
			alert('Please enter a valid quantity.');
			return;
		}

		if (orderItems[productId]) {
			orderItems[productId].quantity += quantity;
		} else {
			orderItems[productId] = {
				name: selectedOption.dataset.name || selectedOption.text,
				price: parseFloat(selectedOption.dataset.price || '0'),
				quantity: quantity
			};
		}

		updateOrderUI();
		productSelect.selectedIndex = 0;
		if (quantityInput) quantityInput.value = 1;
	}

	function handleRemoveProduct(productId) {
		if (orderItems[productId]) {
			delete orderItems[productId];
			updateOrderUI();
		}
	}

	function updateOrderUI() {
		renderOrderTable();
		updateHiddenInputs();
	}

	function renderOrderTable() {
		if (!orderItemsContainer) return;
		if (Object.keys(orderItems).length === 0) {
			orderItemsContainer.innerHTML = '';
			return;
		}

		let total = 0;
		let tableRowsHtml = '';

		for (const id in orderItems) {
			const item = orderItems[id];
			const subtotal = item.quantity * item.price;
			total += subtotal;

			tableRowsHtml += `
			<tr data-product-id="${id}">
				<td>${escapeHtml(item.name)}</td>
				<td class="align-center">${item.quantity}</td>
				<td class="align-right">${item.price.toFixed(2)}</td>
				<td class="align-right">${subtotal.toFixed(2)}</td>
				<td class="align-center">
				<button type="button" class="btn btn-remove">Remove</button>
				</td>
			</tr>`;
		}

		orderItemsContainer.innerHTML = `
		<label class="form-label">Order Items</label>
		<table class="order-items-table">
		<thead>
		<tr>
		<th>Product</th>
		<th class="align-center">Quantity</th>
		<th class="align-right">Unit Price</th>
		<th class="align-right">Subtotal</th>
		<th class="align-center">Action</th>
		</tr>
		</thead>
		<tbody>${tableRowsHtml}</tbody>
		<tfoot>
		<tr>
		<td colspan="3" class="align-right" style="font-weight:600; color: var(--text-light);">Total</td>
		<td class="align-right" style="font-weight:600; color: var(--text-light);">${total.toFixed(2)}</td>
		<td></td>
		</tr>
		</tfoot>
		</table>`;
	}

	function updateHiddenInputs() {
		if (!hiddenInputsContainer) return;
		hiddenInputsContainer.innerHTML = '';
		let index = 0;
		for (const id in orderItems) {
			const item = orderItems[id];
			hiddenInputsContainer.innerHTML +=
				`<input type="hidden" name="OrderItems[${index}].ProductId" value="${escapeAttr(id)}" />\n` +
				`<input type="hidden" name="OrderItems[${index}].Quantity" value="${escapeAttr(item.quantity)}" />`;
			index++;
		}
	}

	function escapeHtml(str) {
		if (typeof str !== 'string') return str;
		return str.replace(/[&<>"']/g, function (c) {
			return {
				'&': '&amp;',
				'<': '&lt;',
				'>': '&gt;',
				'"': '&quot;',
				"'": '&#39;'
			}[c];
		});
	}

	function escapeAttr(val) {
		return String(val).replace(/"/g, '&quot;');
	}
});