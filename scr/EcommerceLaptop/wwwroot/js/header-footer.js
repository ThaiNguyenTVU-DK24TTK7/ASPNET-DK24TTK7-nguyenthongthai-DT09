document.addEventListener('DOMContentLoaded', function () {
    const searchForm = document.querySelector('.search-box');
    const searchInput = document.querySelector('.search-box input');
    const searchIcon = document.querySelector('.search-box i');
    if (searchForm) {
        searchForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const searchTerm = searchInput.value.trim();
            if (searchTerm) {
                console.log('Searching for:', searchTerm);
            }
        });
    }
    if (searchIcon) {
        searchIcon.addEventListener('click', function () {
            const searchTerm = searchInput.value.trim();
            if (searchTerm) {
                console.log('Searching for:', searchTerm);
            }
        });
    }
    function updateCartBadge(count) {
        const cartBadge = document.querySelector('.cart-badge');
        if (cartBadge) {
            cartBadge.textContent = count;
            if (count > 0) {
                cartBadge.style.display = 'flex';
            } else {
                cartBadge.style.display = 'none';
            }
        }
    }
    const currentPage = window.location.pathname;
    const navLinks = document.querySelectorAll('.nav-menu a');

    navLinks.forEach(link => {
        if (link.getAttribute('href') === currentPage.split('/').pop()) {
            link.style.color = '#007bff';
            link.style.fontWeight = '600';
        }
    });
});
document.addEventListener('DOMContentLoaded', function () {
    const subscribeForm = document.querySelector('.email-subscribe');
    const emailInput = document.querySelector('.email-subscribe input');
    const subscribeButton = document.querySelector('.email-subscribe button');

    if (subscribeForm) {
        subscribeForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const email = emailInput.value.trim();

            if (email && isValidEmail(email)) {
                subscribeButton.innerHTML = '<i class="bi bi-arrow-clockwise"></i>';
                subscribeButton.disabled = true;
                setTimeout(() => {
                    subscribeButton.innerHTML = '<i class="bi bi-arrow-right"></i>';
                    subscribeButton.disabled = false;
                    emailInput.value = '';
                    showNotification('Đăng ký thành công! Cảm ơn bạn đã đăng ký nhận tin.', 'success');
                }, 1500);
            } else {
                showNotification('Vui lòng nhập địa chỉ email hợp lệ.', 'error');
            }
        });
    }
    const socialLinks = document.querySelectorAll('.social-icons a');
    socialLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            const platform = this.querySelector('i').className;
            console.log('Social media click:', platform);
        });
    });
    const appButtons = document.querySelectorAll('.app-button');
    appButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            const platform = this.textContent.trim();
            console.log('App download click:', platform);
        });
    });
});
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function showNotification(message, type) {
    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${type === 'success' ? '#28a745' : '#dc3545'};
        color: white;
        padding: 12px 20px;
        border-radius: 8px;
        font-size: 14px;
        font-weight: 500;
        z-index: 1000;
        opacity: 0;
        transform: translateX(100%);
        transition: all 0.3s ease;
        box-shadow: 0 4px 12px rgba(0,0,0,0.2);
        max-width: 300px;
    `;
    notification.textContent = message;

    document.body.appendChild(notification);
    setTimeout(() => {
        notification.style.opacity = '1';
        notification.style.transform = 'translateX(0)';
    }, 100);
    setTimeout(() => {
        notification.style.opacity = '0';
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => {
            if (document.body.contains(notification)) {
                document.body.removeChild(notification);
            }
        }, 300);
    }, 4000);
}
function toggleMobileMenu() {
    const navMenu = document.querySelector('.nav-menu');
    const navContainer = document.querySelector('.nav-container');

    if (navMenu && navContainer) {
        navMenu.classList.toggle('mobile-active');
        navContainer.classList.toggle('mobile-menu-open');
    }
}
document.addEventListener('DOMContentLoaded', function () {
    if (window.innerWidth <= 768) {
        const navLeft = document.querySelector('.nav-left');
        if (navLeft && !document.querySelector('.mobile-menu-btn')) {
            const mobileMenuBtn = document.createElement('button');
            mobileMenuBtn.className = 'mobile-menu-btn';
            mobileMenuBtn.innerHTML = '<i class="bi bi-list"></i>';
            mobileMenuBtn.style.cssText = `
                background: none;
                border: none;
                font-size: 1.5rem;
                color: #333;
                cursor: pointer;
                display: none;
            `;

            mobileMenuBtn.addEventListener('click', toggleMobileMenu);
            navLeft.appendChild(mobileMenuBtn);
            if (window.innerWidth <= 768) {
                mobileMenuBtn.style.display = 'block';
            }
        }
    }
});