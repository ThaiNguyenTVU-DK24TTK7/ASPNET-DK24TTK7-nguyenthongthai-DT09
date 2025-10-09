// Home Page JavaScript - Slider and Product functionality

// Slider functionality
let list = document.querySelector('.slider .list');
let items = document.querySelectorAll('.slider .list .item');
let dots = document.querySelectorAll('.slider .dots li');
let prev = document.getElementById('prev');
let next = document.getElementById('next');

let active = 0;
let lengthItems = items.length - 1;

next.onclick = function(){
    if(active + 1 > lengthItems){
        active = 0;
    }else{
        active = active + 1;
    }
    reloadSlider();
}

prev.onclick = function(){
    if(active - 1 < 0){
        active = lengthItems;
    }else{
        active = active - 1;
    }
    reloadSlider();
}

let refreshSlider = setInterval(()=> {next.click()}, 5000);

function reloadSlider(){
    // Cập nhật class active cho các item
    items.forEach((item, idx) => {
        if(idx === active) {
            item.classList.add('active');
        } else {
            item.classList.remove('active');
        }
    });
    // Cập nhật dot
    let lastActiveDot = document.querySelector('.slider .dots li.active');
    if (lastActiveDot) lastActiveDot.classList.remove('active');
    dots[active].classList.add('active');
    clearInterval(refreshSlider);
    refreshSlider = setInterval(()=> {next.click()}, 5000);
}

dots.forEach((li, key) => {
    li.addEventListener('click', function(){
        active = key;
        reloadSlider();
    })
})

// Favorite functionality
document.addEventListener('DOMContentLoaded', function() {
    console.log('Home.js loaded - Initializing favorite functionality');
    // Thêm event listener cho các button yêu thích
    const favoriteBtns = document.querySelectorAll('.favorite-btn');
    console.log('Found favorite buttons:', favoriteBtns.length);
    favoriteBtns.forEach(btn => {
        btn.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            const productId = this.getAttribute('data-product-id');
            console.log('Favorite button clicked for product ID:', productId);
            if (productId) {
                toggleFavorite(this, productId);
            } else {
                console.error('No product ID found for favorite button');
            }
        });
    });
});

// Toggle yêu thích
function toggleFavorite(element, productId) {
    console.log('Toggling favorite for product ID:', productId);
    // Lấy anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    console.log('Anti-forgery token:', token ? 'Found' : 'Not found');
    // Tạo form data để gửi token đúng cách
    const formData = new FormData();
    formData.append('__RequestVerificationToken', token);
    fetch(`/YeuThich/Toggle/${productId}`, {
        method: 'POST',
        body: formData
    })
    .then(response => {
        console.log('Toggle response status:', response.status);
        return response.json();
    })
    .then(data => {
        console.log('Toggle response data:', data);
        if (data.success) {
            // Reload lại trang Home để đồng bộ trạng thái yêu thích
            setTimeout(() => {
                window.location.reload();
            }, 300);
        } else {
            // Xử lý trường hợp chưa đăng nhập hoặc có lỗi
            if (data.redirectTo) {
                console.log('User not logged in, redirecting to login page...');
                showMessage('Vui lòng đăng nhập để sử dụng tính năng yêu thích!', 'error');
                setTimeout(() => {
                    window.location.href = data.redirectTo;
                }, 1500);
            } else {
                showMessage(data.message || 'Có lỗi xảy ra!', 'error');
            }
        }
    })
    .catch(error => {
        console.error('Error toggling favorite:', error);
        showMessage('Có lỗi xảy ra! Vui lòng thử lại.', 'error');
    });
}

// Hiển thị thông báo
function showMessage(message, type) {
    // Tạo toast notification
    const toast = document.createElement('div');
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 15px 20px;
        border-radius: 5px;
        color: white;
        font-weight: bold;
        z-index: 1000;
        transform: translateX(100%);
        transition: transform 0.3s ease;
    `;
    
    switch(type) {
        case 'success':
            toast.style.backgroundColor = '#28a745';
            break;
        case 'error':
            toast.style.backgroundColor = '#dc3545';
            break;
        case 'info':
            toast.style.backgroundColor = '#17a2b8';
            break;
    }
    
    toast.textContent = message;
    document.body.appendChild(toast);
    
    // Hiển thị toast
    setTimeout(() => {
        toast.style.transform = 'translateX(0)';
    }, 100);
    
    // Tự động xóa sau 3 giây
    setTimeout(() => {
        toast.style.transform = 'translateX(100%)';
        setTimeout(() => {
            if (document.body.contains(toast)) {
                document.body.removeChild(toast);
            }
        }, 300);
    }, 3000);
}

// Product functionality
function initializeProductFeatures() {
    // Product card hover effects
    const productCards = document.querySelectorAll('.product-card');

    productCards.forEach(card => {
        const addButton = card.querySelector('.product-price a');

        // Add to cart functionality
        if (addButton) {
            addButton.addEventListener('click', function (e) {
                e.preventDefault();

                const productTitle = card.querySelector('.product-title').textContent;
                const productPrice = card.querySelector('.discounted-price, .undiscounted-price').textContent;

                // Add animation effect
                this.style.transform = 'scale(0.95)';
                setTimeout(() => {
                    this.style.transform = 'scale(1)';
                }, 150);

                // Show notification
                showNotification(`Đã thêm "${productTitle}" vào giỏ hàng`, 'add');

                // Update cart (you can implement this based on your cart system)
                addToCart({
                    title: productTitle,
                    price: productPrice,
                    image: card.querySelector('.product-image img').src
                });
            });
        }

        // Product image lazy loading
        const productImage = card.querySelector('.product-image img');
        if (productImage) {
            // Add loading placeholder
            productImage.addEventListener('load', function () {
                this.style.opacity = '1';
            });

            productImage.addEventListener('error', function () {
                this.src = 'assets/placeholder.png'; // Fallback image
            });
        }
    });

    // Category navigation
    const categoryItems = document.querySelectorAll('.category-item');
    categoryItems.forEach(item => {
        item.addEventListener('click', function () {
            const categoryName = this.querySelector('span').textContent;

            // Add click animation
            this.style.transform = 'scale(0.98)';
            setTimeout(() => {
                this.style.transform = 'scale(1)';
            }, 150);

            // Navigate to category page (implement based on your routing)
            console.log('Navigating to category:', categoryName);
            // window.location.href = `category.html?cat=${encodeURIComponent(categoryName)}`;
        });
    });
}

// Utility functions for favorites
function getProductId(favoriteButton) {
    const productCard = favoriteButton.closest('.product-card');
    const productTitle = productCard.querySelector('.product-title').textContent;
    return productTitle.replace(/\s+/g, '-').toLowerCase();
}

function addFavorite(productId) {
    let favorites = JSON.parse(localStorage.getItem('favorites') || '[]');
    if (!favorites.includes(productId)) {
        favorites.push(productId);
        localStorage.setItem('favorites', JSON.stringify(favorites));
    }
}

function removeFavorite(productId) {
    let favorites = JSON.parse(localStorage.getItem('favorites') || '[]');
    favorites = favorites.filter(id => id !== productId);
    localStorage.setItem('favorites', JSON.stringify(favorites));
}

function loadFavorites() {
    const favorites = JSON.parse(localStorage.getItem('favorites') || '[]');
    const favoriteButtons = document.querySelectorAll('.favorite-btn');

    favoriteButtons.forEach(button => {
        const productId = getProductId(button);
        if (favorites.includes(productId)) {
            button.classList.add('active');
            button.querySelector('i').className = 'bi bi-heart-fill';
            button.setAttribute('data-tooltip', 'Bỏ khỏi yêu thích');
        }
    });
}

// Cart functionality
function addToCart(product) {
    let cart = JSON.parse(localStorage.getItem('cart') || '[]');

    // Check if product already exists in cart
    const existingProduct = cart.find(item => item.title === product.title);

    if (existingProduct) {
        existingProduct.quantity = (existingProduct.quantity || 1) + 1;
    } else {
        cart.push({
            ...product,
            quantity: 1,
            id: Date.now() // Simple ID generation
        });
    }

    localStorage.setItem('cart', JSON.stringify(cart));
    updateCartBadge();
}

function updateCartBadge() {
    const cart = JSON.parse(localStorage.getItem('cart') || '[]');
    const totalItems = cart.reduce((sum, item) => sum + (item.quantity || 1), 0);

    const cartBadge = document.querySelector('.cart-badge');
    if (cartBadge) {
        cartBadge.textContent = totalItems;
        if (totalItems > 0) {
            cartBadge.style.display = 'flex';
        } else {
            cartBadge.style.display = 'none';
        }
    }
}

// Notification system
function showNotification(message, type) {
    // Create notification element
    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${type === 'add' ? '#28a745' : '#dc3545'};
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
        word-wrap: break-word;
    `;
    notification.textContent = message;

    document.body.appendChild(notification);

    // Show notification
    setTimeout(() => {
        notification.style.opacity = '1';
        notification.style.transform = 'translateX(0)';
    }, 100);

    // Hide and remove notification
    setTimeout(() => {
        notification.style.opacity = '0';
        notification.style.transform = 'translateX(100%)';
        setTimeout(() => {
            if (document.body.contains(notification)) {
                document.body.removeChild(notification);
            }
        }, 300);
    }, 3000);
}

// Smooth scrolling for internal links
function initializeSmoothScrolling() {
    const links = document.querySelectorAll('a[href^="#"]');

    links.forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();

            const targetId = this.getAttribute('href').substring(1);
            const targetElement = document.getElementById(targetId);

            if (targetElement) {
                targetElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });
}

// Performance optimization - Intersection Observer for animations
function initializeScrollAnimations() {
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-in');
            }
        });
    }, observerOptions);

    // Observe product cards and category items
    const elementsToAnimate = document.querySelectorAll('.product-card, .category-item');
    elementsToAnimate.forEach(el => {
        observer.observe(el);
    });
}

// Initialize all home page functionality
function initializeHomePage() {
    // Initialize slider
    initializeSlider();

    // Initialize favorite buttons
    initializeFavoriteButtons();

    // Initialize product features
    initializeProductFeatures();

    // Load saved favorites
    loadFavorites();

    // Update cart badge
    updateCartBadge();

    // Initialize smooth scrolling
    initializeSmoothScrolling();

    // Initialize scroll animations
    if ('IntersectionObserver' in window) {
        initializeScrollAnimations();
    }

    console.log('Home page initialized successfully');
}

// DOM Content Loaded Event
document.addEventListener('DOMContentLoaded', initializeHomePage);

// Window load event for additional setup
window.addEventListener('load', function () {
    // Hide loading spinner if exists
    const loadingSpinner = document.querySelector('.loading-spinner');
    if (loadingSpinner) {
        loadingSpinner.style.display = 'none';
    }

    // Preload next slide images
    if (items.length > 1) {
        const nextSlideIndex = (active + 1) % items.length;
        const nextSlideImg = items[nextSlideIndex].querySelector('img');
        if (nextSlideImg && !nextSlideImg.complete) {
            const img = new Image();
            img.src = nextSlideImg.src;
        }
    }
});

// Handle page visibility change (pause/resume slider)
document.addEventListener('visibilitychange', function () {
    if (document.hidden) {
        stopAutoSlide();
    } else {
        startAutoSlide();
    }
});

// Keyboard navigation for slider
document.addEventListener('keydown', function (e) {
    if (e.key === 'ArrowLeft') {
        prevSlide();
        stopAutoSlide();
        startAutoSlide();
    } else if (e.key === 'ArrowRight') {
        nextSlide();
        stopAutoSlide();
        startAutoSlide();
    }
});

// Export functions for potential use in other scripts
window.homePageFunctions = {
    showSlide,
    nextSlide,
    prevSlide,
    addToCart,
    updateCartBadge,
    showNotification
};