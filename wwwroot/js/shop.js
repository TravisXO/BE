document.addEventListener('DOMContentLoaded', function () {
    // === Price Slider Logic ===
    const minPriceSlider = document.getElementById('minPriceSlider');
    const maxPriceSlider = document.getElementById('maxPriceSlider');
    const minPriceInput = document.getElementById('minPriceInput');
    const maxPriceInput = document.getElementById('maxPriceInput');
    const priceFilterForm = document.getElementById('priceFilterForm');
    const sliderTrack = document.querySelector('.price-slider-wrapper .slider-track');

    // Only proceed with price slider if all elements exist
    if (minPriceSlider && maxPriceSlider && minPriceInput && maxPriceInput && priceFilterForm && sliderTrack) {
        const initialMin = parseFloat(minPriceSlider.min);
        const initialMax = parseFloat(maxPriceSlider.max);

        // Function to update slider track and input values
        function updateSliderAndInputs() {
            let minVal = parseFloat(minPriceSlider.value);
            let maxVal = parseFloat(maxPriceSlider.value);

            // Ensure min is always less than or equal to max, and vice-versa
            if (minVal > maxVal) {
                [minVal, maxVal] = [maxVal, minVal]; // Swap values
                minPriceSlider.value = minVal; // Update slider to reflect swap
                maxPriceSlider.value = maxVal;
            }

            minPriceInput.value = minVal.toFixed(2);
            maxPriceInput.value = maxVal.toFixed(2);

            // Calculate percentage for slider track fill
            const range = initialMax - initialMin;
            const minPercent = ((minVal - initialMin) / range) * 100;
            const maxPercent = ((maxVal - initialMin) / range) * 100;

            sliderTrack.style.left = minPercent + '%';
            sliderTrack.style.width = (maxPercent - minPercent) + '%';
        }

        // Event Listeners for sliders
        minPriceSlider.addEventListener('input', updateSliderAndInputs);
        maxPriceSlider.addEventListener('input', updateSliderAndInputs);

        // Event Listeners for direct input boxes
        minPriceInput.addEventListener('change', function () {
            let val = parseFloat(this.value);
            if (isNaN(val) || val < initialMin) val = initialMin;
            if (val > parseFloat(maxPriceInput.value)) val = parseFloat(maxPriceInput.value); // Prevent min > max
            minPriceSlider.value = val;
            updateSliderAndInputs();
        });

        maxPriceInput.addEventListener('change', function () {
            let val = parseFloat(this.value);
            if (isNaN(val) || val > initialMax) val = initialMax;
            if (val < parseFloat(minPriceInput.value)) val = parseFloat(minPriceInput.value); // Prevent max < min
            maxPriceSlider.value = val;
            updateSliderAndInputs();
        });

        // Initial update to set correct positions on load
        updateSliderAndInputs();
    }


    // === Sliding Cart Menu Logic ===
    const slidingCart = document.getElementById('slidingCart');
    const closeCartBtn = document.getElementById('closeCartBtn');
    const cartItemsList = document.querySelector('#slidingCart .cart-items-list');
    const cartSubtotalElement = document.getElementById('cartSubtotal');
    const headerCartIcon = document.querySelector('.nav-icons a[href="/Cart/Index"]'); // Select the cart icon in the header

    // Create a new overlay element if it doesn't exist
    let cartOverlay = document.querySelector('.cart-overlay');
    if (!cartOverlay) {
        cartOverlay = document.createElement('div');
        cartOverlay.classList.add('cart-overlay');
        document.body.appendChild(cartOverlay);
    }

    // Add a div for displaying temporary messages at the top of the sliding cart
    let cartMessageDisplay = document.getElementById('cartMessageDisplay');
    if (!cartMessageDisplay) {
        cartMessageDisplay = document.createElement('div');
        cartMessageDisplay.id = 'cartMessageDisplay';
        cartMessageDisplay.style.cssText = 'padding: 10px; margin: 10px; border-radius: 5px; text-align: center; display: none;'; // Initial styling
        const offcanvasHeader = document.querySelector('#slidingCart .offcanvas-header');
        if (offcanvasHeader) {
            offcanvasHeader.after(cartMessageDisplay); // Insert after the header
        } else {
            slidingCart.prepend(cartMessageDisplay); // Fallback to prepend if header not found
        }
    }

    // Function to display temporary messages in the cart
    function showCartMessage(message, isSuccess) {
        if (!cartMessageDisplay) { // Fallback if element not found (shouldn't happen with the check above)
            console.warn('cartMessageDisplay element not found for message:', message);
            return;
        }

        cartMessageDisplay.textContent = message;
        cartMessageDisplay.style.display = 'block';
        if (isSuccess) {
            cartMessageDisplay.style.backgroundColor = '#d4edda'; // Light green for success
            cartMessageDisplay.style.color = '#155724'; // Dark green text
        } else {
            cartMessageDisplay.style.backgroundColor = '#f8d7da'; // Light red for error
            cartMessageDisplay.style.color = '#721c24'; // Dark red text
        }
        setTimeout(() => {
            cartMessageDisplay.style.display = 'none';
        }, 3000); // Hide after 3 seconds
    }


    function toggleSlidingCart() {
        if (slidingCart.classList.contains('open')) {
            slidingCart.classList.remove('open');
            cartOverlay.classList.remove('visible');
            document.body.style.overflow = ''; // Restore scrolling
            // Hide any lingering messages when closing the cart
            if (cartMessageDisplay) {
                cartMessageDisplay.style.display = 'none';
            }
        } else {
            fetchCartContents(); // Load fresh cart data when opening
            // Add a small delay to ensure rendering is stable before opening the cart
            setTimeout(() => {
                slidingCart.classList.add('open');
                cartOverlay.classList.add('visible');
                document.body.style.overflow = 'hidden'; // Prevent body scrolling when cart is open
            }, 50); // Small delay, e.g., 50ms
        }
    }

    // Function to fetch and render cart contents
    async function fetchCartContents() {
        try {
            // Use the global variable defined in the .cshtml file
            const response = await fetch(getCartDataUrl);
            const data = await response.json();

            cartItemsList.innerHTML = ''; // Clear previous items

            if (data.hasItems) {
                data.items.forEach(item => {
                    const itemHtml = `
                        <div class="cart-item-small" data-product-id="${item.productId}">
                            <img src="${item.imageUrl}" alt="${item.productName}" class="item-img">
                            <div class="item-details-small">
                                <h4>${item.productName}</h4>
                                <p class="price-qty">R ${item.price.toFixed(2)} x <span class="item-quantity-display">${item.quantity}</span> = <span class="item-total-price">R ${item.totalPrice.toFixed(2)}</span></p>
                                <div class="quantity-controls">
                                    <button class="quantity-btn decrease-qty" data-product-id="${item.productId}" data-current-qty="${item.quantity}">-</button>
                                    <span class="qty-display">${item.quantity}</span>
                                    <button class="quantity-btn increase-qty" data-product-id="${item.productId}" data-current-qty="${item.quantity}">+</button>
                                </div>
                            </div>
                            <button class="item-remove-small" data-product-id="${item.productId}">&times;</button>
                        </div>
                    `;
                    cartItemsList.insertAdjacentHTML('beforeend', itemHtml);
                });
                cartSubtotalElement.textContent = `R ${data.subtotal.toFixed(2)}`;
            } else {
                cartItemsList.innerHTML = '<p class="empty-cart-message">Your cart is empty.</p>';
                cartSubtotalElement.textContent = `R 0.00`;
            }

            // Re-attach event listeners for remove buttons after rendering
            cartItemsList.querySelectorAll('.item-remove-small').forEach(button => {
                // Remove existing listeners to prevent multiple bindings
                button.removeEventListener('click', handleSmallCartRemove);
                button.addEventListener('click', handleSmallCartRemove);
            });

            // Re-attach event listeners for quantity buttons
            cartItemsList.querySelectorAll('.quantity-btn').forEach(button => {
                // Remove existing listeners to prevent multiple bindings
                button.removeEventListener('click', handleSmallCartQuantityChange);
                button.addEventListener('click', handleSmallCartQuantityChange);
            });

        } catch (error) {
            console.error('Error fetching cart contents:', error);
            cartItemsList.innerHTML = '<p class="empty-cart-message" style="color: red;">Error loading cart. Please try again.</p>';
            cartSubtotalElement.textContent = `R 0.00`;
        }
    }

    // Named handler for small cart remove buttons
    async function handleSmallCartRemove() {
        const productId = this.dataset.productId;
        await removeFromCartAjax(productId); // Now uses fetch
        fetchCartContents(); // Refresh cart after removal
    }

    // Named handler for small cart quantity buttons
    async function handleSmallCartQuantityChange() {
        const productId = this.dataset.productId;
        let currentQty = parseInt(this.dataset.currentQty);
        const isIncrease = this.classList.contains('increase-qty');

        let newQuantity = isIncrease ? currentQty + 1 : currentQty - 1;

        // Prevent quantity from going below 0 (item should be removed instead by backend logic)
        if (newQuantity < 0) newQuantity = 0;

        await updateCartQuantityAjax(productId, newQuantity); // Now uses fetch
        fetchCartContents(); // Refresh cart after quantity update
    }


    // Function to send remove request via AJAX (NOW USES FETCH)
    async function removeFromCartAjax(productId) {
        const formData = new FormData();
        formData.append('productId', productId);

        const antiForgeryTokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (antiForgeryTokenInput) {
            formData.append('__RequestVerificationToken', antiForgeryTokenInput.value);
        } else {
            console.warn('Anti-forgery token not found for remove. Request might fail.');
        }

        try {
            const response = await fetch(removeFromCartUrl, { // Use fetch here
                method: 'POST',
                body: formData
            });

            const contentType = response.headers.get("content-type");
            if (contentType && contentType.indexOf("application/json") !== -1) {
                const data = await response.json();
                if (!data.success) {
                    showCartMessage(data.message, false); // Alert if server indicates failure
                } else {
                    showCartMessage(data.message, true); // Show success message
                }
            } else {
                console.error('Expected JSON response for remove, but received:', contentType);
                showCartMessage('An unexpected error occurred during removal. Please try again.', false);
            }
        } catch (error) {
            console.error('Error removing from cart:', error);
            showCartMessage('Failed to remove item from cart. See console for details.', false);
        }
    }

    // Function to send quantity update request via AJAX (NOW USES FETCH)
    async function updateCartQuantityAjax(productId, newQuantity) {
        const formData = new FormData();
        formData.append('productId', productId);
        formData.append('newQuantity', newQuantity);

        const antiForgeryTokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (antiForgeryTokenInput) {
            formData.append('__RequestVerificationToken', antiForgeryTokenInput.value);
        } else {
            console.warn('Anti-forgery token not found for quantity update. Request might fail.');
        }

        try {
            const response = await fetch(updateQuantityUrl, { // Use fetch here
                method: 'POST',
                body: formData
            });

            const contentType = response.headers.get("content-type");
            if (contentType && contentType.indexOf("application/json") !== -1) {
                const data = await response.json();
                if (!data.success) {
                    showCartMessage(data.message, false); // Alert if server indicates failure
                } else {
                    showCartMessage(data.message, true); // Show success message
                }
            } else {
                console.error('Expected JSON response for quantity update, but received:', contentType);
                showCartMessage('An unexpected error occurred during quantity update. Please try again.', false);
            }
        } catch (error) {
            console.error('Error updating cart quantity:', error);
            showCartMessage('Failed to update item quantity. See console for details.', false);
        }
    }


    // Event listener for opening the cart (e.g., clicking header cart icon)
    if (headerCartIcon) {
        headerCartIcon.addEventListener('click', function (event) {
            event.preventDefault(); // Prevent default link behavior
            toggleSlidingCart();
        });
    }


    // Event listener for closing the cart
    if (closeCartBtn) {
        closeCartBtn.addEventListener('click', toggleSlidingCart);
    }
    // Close cart when clicking outside the menu
    if (cartOverlay) {
        cartOverlay.addEventListener('click', toggleSlidingCart);
    }


    // Modify existing AddToCart form submission
    const addToCartForms = document.querySelectorAll('.add-to-cart-form');
    addToCartForms.forEach(form => {
        const submitButton = form.querySelector('button[type="submit"]'); // Get the submit button once per form
        const originalButtonHtml = submitButton ? submitButton.innerHTML : ''; // Store original HTML

        form.addEventListener('submit', async function (event) {
            event.preventDefault(); // Prevent default form submission (page reload)

            if (submitButton) { // Ensure button exists before trying to disable
                submitButton.disabled = true; // Disable the button to prevent multiple clicks
                // Check if button has inner HTML (like an icon) or just text
                if (submitButton.children.length > 0) { // Has children (e.g., <i> icon)
                    const icon = submitButton.querySelector('i');
                    if (icon) icon.style.display = 'none'; // Hide icon temporarily
                    submitButton.innerHTML = (icon ? icon.outerHTML : '') + 'Adding...'; // Add current icon HTML back
                } else {
                    submitButton.textContent = 'Adding...'; // Just change text
                }
            }

            const productId = this.querySelector('input[name="productId"]').value;
            const antiforgeryToken = this.querySelector('input[name="__RequestVerificationToken"]').value;

            const formData = new FormData();
            formData.append('productId', productId);
            formData.append('__RequestVerificationToken', antiforgeryToken);

            try {
                const response = await fetch(addToCartUrl, {
                    method: 'POST',
                    body: formData
                });

                const contentType = response.headers.get("content-type");
                if (contentType && contentType.indexOf("application/json") !== -1) {
                    const data = await response.json();
                    if (data.success) {
                        await fetchCartContents();
                        toggleSlidingCart();
                        showCartMessage(data.message, true);
                    } else {
                        if (data.message && data.message.includes("You must be logged in")) {
                            window.location.href = '/Identity/Account/Login'; // Redirect to login page
                        } else {
                            showCartMessage(data.message, false);
                        }
                    }
                } else {
                    console.error('Expected JSON response, but received:', contentType);
                    showCartMessage('An unexpected error occurred. Please try again.', false);
                }
            } catch (error) {
                console.error('Error adding to cart:', error);
                showCartMessage('Failed to add item to cart. See console for details.', false);
            } finally {
                if (submitButton) { // Re-enable the button and restore original HTML
                    submitButton.disabled = false;
                    submitButton.innerHTML = originalButtonHtml; // Restore original HTML content
                }
            }
        });
    });

    // Initial fetch of cart contents on page load if needed (e.g., if cart count should be displayed)
    // fetchCartContents(); // Uncomment if you want the cart to load on page refresh even if not opened
});