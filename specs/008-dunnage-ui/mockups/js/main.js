// Main Window Navigation
document.addEventListener('DOMContentLoaded', () => {
    // Handle navigation item clicks
    const navItems = document.querySelectorAll('.nav-item');
    navItems.forEach(item => {
        item.addEventListener('click', (e) => {
            // Remove active class from all items
            navItems.forEach(i => i.classList.remove('active'));
            // Add active class to clicked item
            item.classList.add('active');
        });
    });

    // Update page title based on active nav item
    const updatePageTitle = () => {
        const activeNav = document.querySelector('.nav-item.active');
        if (activeNav) {
            const title = activeNav.querySelector('.nav-text').textContent;
            const pageTitle = document.getElementById('pageTitle');
            if (pageTitle) {
                pageTitle.textContent = title;
            }
        }
    };

    updatePageTitle();

    // Handle user display (could be dynamic in real app)
    const userDisplay = document.getElementById('userDisplay');
    if (userDisplay) {
        // In real app, this would come from authentication service
        userDisplay.textContent = 'John Doe';
    }
});

// Listen for messages from iframe to update page title
window.addEventListener('message', (event) => {
    if (event.data.type === 'updateTitle') {
        const pageTitle = document.getElementById('pageTitle');
        if (pageTitle) {
            pageTitle.textContent = event.data.title;
        }
    }
});
