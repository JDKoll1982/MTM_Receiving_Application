const detailsToggle = document.getElementById("toggle-details");
const detailItems = Array.from(document.querySelectorAll(".change-item"));
const filterButtons = Array.from(document.querySelectorAll(".filter-chip"));
const backToTopButton = document.getElementById("back-to-top");
const copyVersionButton = document.getElementById("copy-version");
const versionBadge = document.querySelector(".badge");
const scrollButton = document.querySelector("[data-scroll-target]");

let expanded = true;

const setAllDetails = (isOpen) => {
    detailItems.forEach((item) => {
        item.open = isOpen;
    });
    expanded = isOpen;
    if (detailsToggle) {
        detailsToggle.textContent = isOpen ? "Collapse all details" : "Expand all details";
        detailsToggle.setAttribute("aria-expanded", String(isOpen));
    }
};

if (detailsToggle) {
    detailsToggle.addEventListener("click", () => {
        setAllDetails(!expanded);
    });
    setAllDetails(true);
}

filterButtons.forEach((button) => {
    button.addEventListener("click", () => {
        const selectedFilter = button.dataset.filter;

        filterButtons.forEach((item) => item.classList.remove("is-active"));
        button.classList.add("is-active");

        detailItems.forEach((item) => {
            const matches = selectedFilter === "all" || item.dataset.category === selectedFilter;
            item.hidden = !matches;
        });
    });
});

if (scrollButton) {
    scrollButton.addEventListener("click", () => {
        const target = document.querySelector(scrollButton.dataset.scrollTarget);
        if (target) {
            target.scrollIntoView({ behavior: "smooth", block: "start" });
        }
    });
}

if (copyVersionButton && versionBadge) {
    copyVersionButton.addEventListener("click", async () => {
        const versionText = versionBadge.textContent.trim();
        try {
            await navigator.clipboard.writeText(versionText);
            copyVersionButton.textContent = "Version copied";
            window.setTimeout(() => {
                copyVersionButton.textContent = "Copy version";
            }, 1600);
        } catch {
            copyVersionButton.textContent = versionText;
        }
    });
}

const updateBackToTopVisibility = () => {
    if (!backToTopButton) {
        return;
    }

    if (window.scrollY > 320) {
        backToTopButton.classList.add("is-visible");
    } else {
        backToTopButton.classList.remove("is-visible");
    }
};

if (backToTopButton) {
    backToTopButton.addEventListener("click", () => {
        window.scrollTo({ top: 0, behavior: "smooth" });
    });
}

window.addEventListener("scroll", updateBackToTopVisibility, { passive: true });
updateBackToTopVisibility();