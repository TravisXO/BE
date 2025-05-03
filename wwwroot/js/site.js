// User Icon Drop-down Toggle 
function toggleUserMenu() {
    document.getElementById("user-menu").classList.toggle("hidden");
}

document.addEventListener("click", function (e) {
    const wrapper = document.querySelector(".user-dropdown-wrapper");
    if (!wrapper.contains(e.target)) {
        document.getElementById("user-menu").classList.add("hidden");
    }
});