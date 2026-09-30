
document.addEventListener('DOMContentLoaded', function () {
	refreshHeaderCounts();

	document.querySelectorAll('.ajax-cart-btn').forEach(button => {
		button.addEventListener('click', function (event) {
			event.preventDefault();
			event.stopPropagation();
			addProductToCart(this.dataset.productId, this);
		});
	});
});

function refreshHeaderCounts() {
	Promise.all([
		fetch('/GioHang/Count', { credentials: 'same-origin' }).then(response => response.json()),
		fetch('/YeuThich/Count', { credentials: 'same-origin' }).then(response => response.json())
	]).then(([cart, favorites]) => {
		updateHeaderCount('.cart-badge', cart.count);
		updateHeaderCount('.favorite-badge', favorites.count);
	}).catch(() => {
		updateHeaderCount('.cart-badge', 0);
		updateHeaderCount('.favorite-badge', 0);
	});
}

function updateHeaderCount(selector, count) {
	const badge = document.querySelector(selector);
	if (!badge) return;

	badge.textContent = count;
	badge.style.display = count > 0 ? 'flex' : 'none';
	badge.classList.remove('is-updated');
	void badge.offsetWidth;
	if (count > 0) badge.classList.add('is-updated');
}

function animateAction(element) {
	if (!element) return;
	element.classList.remove('action-success');
	void element.offsetWidth;
	element.classList.add('action-success');
}

function addProductToCart(productId, button, quantity = 1) {
	const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
	return fetch('/GioHang/AddToCart', {
		method: 'POST',
		headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token || '' },
		body: JSON.stringify({ sanPhamId: productId, quantity }),
		credentials: 'same-origin'
	}).then(response => response.json()).then(data => {
		if (data.success) {
			updateHeaderCount('.cart-badge', data.cartCount);
			animateAction(button);
			showActionMessage(data.message || 'Đã thêm sản phẩm vào giỏ hàng.', 'success');
		} else if (data.redirectTo) {
			window.location.href = data.redirectTo;
		} else {
			showActionMessage(data.message || 'Không thể thêm sản phẩm vào giỏ hàng.', 'error');
		}
		return data;
	});
}

function toggleFavoriteAjax(button, productId) {
	const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
	const formData = new FormData();
	formData.append('__RequestVerificationToken', token || '');

	return fetch(`/YeuThich/Toggle/${productId}`, {
		method: 'POST',
		body: formData,
		credentials: 'same-origin'
	}).then(response => response.json()).then(data => {
		if (data.success) {
			button.classList.toggle('active', data.favorited);
			button.classList.toggle('favorited', data.favorited);
			const icon = button.querySelector('i');
			if (icon) icon.className = data.favorited ? 'bi bi-heart-fill' : 'bi bi-heart';
			button.setAttribute('data-tooltip', data.favorited ? 'Bỏ khỏi yêu thích' : 'Thêm vào yêu thích');
			updateHeaderCount('.favorite-badge', data.favoriteCount);
			animateAction(button);
			showActionMessage(data.favorited ? 'Đã thêm vào yêu thích.' : 'Đã xóa khỏi yêu thích.', 'success');
		} else if (data.redirectTo) {
			window.location.href = data.redirectTo;
		} else {
			showActionMessage(data.message || 'Không thể cập nhật yêu thích.', 'error');
		}
		return data;
	});
}

function showActionMessage(message, type) {
	const toast = document.createElement('div');
	toast.className = `action-toast action-toast-${type}`;
	toast.textContent = message;
	document.body.appendChild(toast);
	requestAnimationFrame(() => toast.classList.add('visible'));
	setTimeout(() => {
		toast.classList.remove('visible');
		setTimeout(() => toast.remove(), 250);
	}, 2200);
}
