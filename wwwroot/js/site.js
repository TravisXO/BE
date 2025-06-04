// User Icon Drop-down Toggle 
function toggleUserMenu() {
    document.getElementById("user-menu").classList.toggle("hidden");
}

document.addEventListener("click", function (e) {
    const wrapper = document.querySelector(".user-dropdown-wrapper");
    if (wrapper && !wrapper.contains(e.target)) { // Check if wrapper exists
        const userMenu = document.getElementById("user-menu");
        if (userMenu) { // Check if userMenu exists
            userMenu.classList.add("hidden");
        }
    }
});

// Mega Menu Toggle
document.addEventListener("DOMContentLoaded", function () {
    const sidebarHeader = document.getElementById("sidebarHeader");
    const megaMenu = document.getElementById("megaMenu");
    const closeMegaMenuBtn = document.getElementById("closeMegaMenuBtn");

    const bagsAndBackpacksMenuItem = document.getElementById("bagsAndBackpacksMenuItem");
    const megaSubmenu = document.getElementById("megaSubmenu");

    const covidProductsMenuItem = document.getElementById("covidProductsMenuItem");
    const covidProductsSubmenu = document.getElementById("covidProductsSubmenu");

    const clothingMenuItem = document.getElementById("clothingMenuItem");
    const clothingSubmenu = document.getElementById("clothingSubmenu");

    const customPackagingMenuItem = document.getElementById("customPackagingMenuItem");
    const customPackagingSubmenu = document.getElementById("customPackagingSubmenu");

    const drinkwareMenuItem = document.getElementById("drinkwareMenuItem");
    const drinkwareSubmenu = document.getElementById("drinkwareSubmenu");

    const gadgetsTechMenuItem = document.getElementById("gadgetsTechMenuItem");
    const gadgetsTechSubmenu = document.getElementById("gadgetsTechSubmenu");

    const homeOutdoorMenuItem = document.getElementById("homeOutdoorMenuItem");
    const homeOutdoorSubmenu = document.getElementById("homeOutdoorSubmenu");

    const miscellaneousMenuItem = document.getElementById("miscellaneousMenuItem");
    const miscellaneousSubmenu = document.getElementById("miscellaneousSubmenu");

    const officeMenuItem = document.getElementById("officeMenuItem");
    const officeSubmenu = document.getElementById("officeSubmenu");

    const sportFitnessMenuItem = document.getElementById("sportFitnessMenuItem");
    const sportFitnessSubmenu = document.getElementById("sportFitnessSubmenu");

    const tradeshowsEventsMenuItem = document.getElementById("tradeshowsEventsMenuItem");
    const tradeshowsEventsSubmenu = document.getElementById("tradeshowsEventsSubmenu");

    // Function to hide all submenus
    function hideAllSubmenus() {
        if (megaSubmenu) megaSubmenu.classList.remove("active");
        if (covidProductsSubmenu) covidProductsSubmenu.classList.remove("active");
        if (clothingSubmenu) clothingSubmenu.classList.remove("active");
        if (customPackagingSubmenu) customPackagingSubmenu.classList.remove("active");
        if (drinkwareSubmenu) drinkwareSubmenu.classList.remove("active");
        if (gadgetsTechSubmenu) gadgetsTechSubmenu.classList.remove("active");
        if (homeOutdoorSubmenu) homeOutdoorSubmenu.classList.remove("active");
        if (miscellaneousSubmenu) miscellaneousSubmenu.classList.remove("active");
        if (officeSubmenu) officeSubmenu.classList.remove("active");
        if (sportFitnessSubmenu) sportFitnessSubmenu.classList.remove("active"); // New
        if (tradeshowsEventsSubmenu) tradeshowsEventsSubmenu.classList.remove("active"); // New
    }

    if (sidebarHeader && megaMenu && closeMegaMenuBtn) { // Check if main elements exist
        sidebarHeader.addEventListener("click", function () {
            megaMenu.classList.add("active");
            hideAllSubmenus(); // Hide all sub-menus when main menu opens
        });

        closeMegaMenuBtn.addEventListener("click", function () {
            megaMenu.classList.remove("active");
            hideAllSubmenus(); // Hide all sub-menus when main menu closes
        });

        // Close mega menu when clicking outside of it
        document.addEventListener("click", function (event) {
            // Check if click is outside megaMenu AND outside sidebarHeader AND megaMenu is active
            if (!megaMenu.contains(event.target) && !sidebarHeader.contains(event.target) && megaMenu.classList.contains("active")) {
                megaMenu.classList.remove("active");
                hideAllSubmenus(); // Hide all sub-menus as well
            }
        });
    }

    // Logic for Bags & Backpacks submenu hover
    if (bagsAndBackpacksMenuItem && megaSubmenu) {
        bagsAndBackpacksMenuItem.addEventListener("mouseenter", function () {
            if (megaMenu.classList.contains("active")) { // Only show submenu if main mega menu is active
                hideAllSubmenus(); // Hide other submenus before showing this one
                megaSubmenu.classList.add("active");
            }
        });

        bagsAndBackpacksMenuItem.addEventListener("mouseleave", function (event) {
            if (!megaSubmenu.contains(event.relatedTarget)) {
                megaSubmenu.classList.remove("active");
            }
        });

        megaSubmenu.addEventListener("mouseenter", function () {
            megaSubmenu.classList.add("active");
        });

        megaSubmenu.addEventListener("mouseleave", function () {
            megaSubmenu.classList.remove("active");
        });
    }

    // Logic for COVID-19 Products submenu hover
    if (covidProductsMenuItem && covidProductsSubmenu) {
        covidProductsMenuItem.addEventListener("mouseenter", function () {
            if (megaMenu.classList.contains("active")) {
                hideAllSubmenus();
                covidProductsSubmenu.classList.add("active");
            }
        });

        covidProductsMenuItem.addEventListener("mouseleave", function (event) {
            if (!covidProductsSubmenu.contains(event.relatedTarget)) {
                covidProductsSubmenu.classList.remove("active");
            }
        });

        covidProductsSubmenu.addEventListener("mouseenter", function () {
            covidProductsSubmenu.classList.add("active");
        });

        covidProductsSubmenu.addEventListener("mouseleave", function () {
            covidProductsSubmenu.classList.remove("active");
        });
    }

    // Logic for Clothing submenu hover
    if (clothingMenuItem && clothingSubmenu) {
        clothingMenuItem.addEventListener("mouseenter", function () {
            if (megaMenu.classList.contains("active")) {
                hideAllSubmenus();
                clothingSubmenu.classList.add("active");
            }
        });

        clothingMenuItem.addEventListener("mouseleave", function (event) {
            if (!clothingSubmenu.contains(event.relatedTarget)) {
                clothingSubmenu.classList.remove("active");
            }
        });

        clothingSubmenu.addEventListener("mouseenter", function () {
            clothingSubmenu.classList.add("active");
        });

        clothingSubmenu.addEventListener("mouseleave", function () {
            clothingSubmenu.classList.remove("active");
        });
    }

    // Logic for Custom Packaging submenu hover
    if (customPackagingMenuItem && customPackagingSubmenu) {
        customPackagingMenuItem.addEventListener("mouseenter", function () {
            if (megaMenu.classList.contains("active")) {
                hideAllSubmenus();
                customPackagingSubmenu.classList.add("active");
            }
        });

        customPackagingMenuItem.addEventListener("mouseleave", function (event) {
            if (!customPackagingSubmenu.contains(event.relatedTarget)) {
                customPackagingSubmenu.classList.remove("active");
            }
        });

        customPackagingSubmenu.addEventListener("mouseenter", function () {
            customPackagingSubmenu.classList.add("active");
        });

        customPackagingSubmenu.addEventListener("mouseleave", function () {
            customPackagingSubmenu.classList.remove("active");
        });
    }

    // Logic for Drinkware submenu hover
    if (drinkwareMenuItem && drinkwareSubmenu) {
        drinkwareMenuItem.addEventListener("mouseenter", function () {
            if (megaMenu.classList.contains("active")) {
                hideAllSubmenus();
                drinkwareSubmenu.classList.add("active");
            }
        });

        drinkwareMenuItem.addEventListener("mouseleave", function (event) {
            if (!drinkwareSubmenu.contains(event.relatedTarget)) {
                drinkwareSubmenu.classList.remove("active");
            }
        });

        drinkwareSubmenu.addEventListener("mouseenter", function () {
            drinkwareSubmenu.classList.add("active");
        });

        drinkwareSubmenu.addEventListener("mouseleave", function () {
            drinkwareSubmenu.classList.remove("active");
        });
    }

    // Logic for Gadgets & Tech submenu hover
    if (gadgetsTechMenuItem && gadgetsTechSubmenu) {
        gadgetsTechMenuItem.addEventListener("mouseenter", function () {
            if (megaMenu.classList.contains("active")) {
                hideAllSubmenus();
                gadgetsTechSubmenu.classList.add("active");
            }
        });

        gadgetsTechMenuItem.addEventListener("mouseleave", function (event) {
            if (!gadgetsTechSubmenu.contains(event.relatedTarget)) {
                gadgetsTechSubmenu.classList.remove("active");
            }
        });

        gadgetsTechSubmenu.addEventListener("mouseenter", function () {
            gadgetsTechSubmenu.classList.add("active");
        });

        gadgetsTechSubmenu.addEventListener("mouseleave", function () {
            gadgetsTechSubmenu.classList.remove("active");
        });
    }

    // Logic for Home & Outdoor submenu hover
    if (homeOutdoorMenuItem && homeOutdoorSubmenu) {
        homeOutdoorMenuItem.addEventListener("mouseenter", function () {
            if (megaMenu.classList.contains("active")) {
                hideAllSubmenus();
                homeOutdoorSubmenu.classList.add("active");
            }
        });

        homeOutdoorMenuItem.addEventListener("mouseleave", function (event) {
            if (!homeOutdoorSubmenu.contains(event.relatedTarget)) {
                homeOutdoorSubmenu.classList.remove("active");
            }
        });

        homeOutdoorSubmenu.addEventListener("mouseenter", function () {
            homeOutdoorSubmenu.classList.add("active");
        });

        homeOutdoorSubmenu.addEventListener("mouseleave", function () {
            homeOutdoorSubmenu.classList.remove("active");
        });
    }

    // Logic for Miscellaneous submenu hover
    if (miscellaneousMenuItem && miscellaneousSubmenu) {
        miscellaneousMenuItem.addEventListener("mouseenter", function () {
            if (megaMenu.classList.contains("active")) {
                hideAllSubmenus();
                miscellaneousSubmenu.classList.add("active");
            }
        });

        miscellaneousMenuItem.addEventListener("mouseleave", function (event) {
            if (!miscellaneousSubmenu.contains(event.relatedTarget)) {
                miscellaneousSubmenu.classList.remove("active");
            }
        });

        miscellaneousSubmenu.addEventListener("mouseenter", function () {
            miscellaneousSubmenu.classList.add("active");
        });

        miscellaneousSubmenu.addEventListener("mouseleave", function () {
            miscellaneousSubmenu.classList.remove("active");
        });
    }

    // Logic for Office submenu hover
    if (officeMenuItem && officeSubmenu) {
        officeMenuItem.addEventListener("mouseenter", function () {
            if (megaMenu.classList.contains("active")) {
                hideAllSubmenus();
                officeSubmenu.classList.add("active");
            }
        });

        officeMenuItem.addEventListener("mouseleave", function (event) {
            if (!officeSubmenu.contains(event.relatedTarget)) {
                officeSubmenu.classList.remove("active");
            }
        });

        officeSubmenu.addEventListener("mouseenter", function () {
            officeSubmenu.classList.add("active");
        });

        officeSubmenu.addEventListener("mouseleave", function () {
            officeSubmenu.classList.remove("active");
        });
    }

    // Logic for Sport & Fitness submenu hover
    if (sportFitnessMenuItem && sportFitnessSubmenu) {
        sportFitnessMenuItem.addEventListener("mouseenter", function () {
            if (megaMenu.classList.contains("active")) {
                hideAllSubmenus();
                sportFitnessSubmenu.classList.add("active");
            }
        });

        sportFitnessMenuItem.addEventListener("mouseleave", function (event) {
            if (!sportFitnessSubmenu.contains(event.relatedTarget)) {
                sportFitnessSubmenu.classList.remove("active");
            }
        });

        sportFitnessSubmenu.addEventListener("mouseenter", function () {
            sportFitnessSubmenu.classList.add("active");
        });

        sportFitnessSubmenu.addEventListener("mouseleave", function () {
            sportFitnessSubmenu.classList.remove("active");
        });
    }

    // site.js

    // Logic for Tradeshows & Events submenu hover
    if (tradeshowsEventsMenuItem && tradeshowsEventsSubmenu) {
        tradeshowsEventsMenuItem.addEventListener("mouseenter", function () {
            if (megaMenu.classList.contains("active")) {
                hideAllSubmenus();
                tradeshowsEventsSubmenu.classList.add("active");
            }
        });

        tradeshowsEventsMenuItem.addEventListener("mouseleave", function (event) {
            if (!tradeshowsEventsSubmenu.contains(event.relatedTarget)) {
                tradeshowsEventsSubmenu.classList.remove("active");
            }
        });

        tradeshowsEventsSubmenu.addEventListener("mouseenter", function () {
            tradeshowsEventsSubmenu.classList.add("active");
        });

        tradeshowsEventsSubmenu.addEventListener("mouseleave", function () {
            tradeshowsEventsSubmenu.classList.remove("active");
        });
    }
});