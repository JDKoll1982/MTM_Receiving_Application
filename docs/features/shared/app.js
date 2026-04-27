/* ============================================================
   MTM Feature Docs — Shared JavaScript
   Applies to:  docs/features/ * /index.html
                docs/database/ * /index.html
   ============================================================ */

(function () {
    "use strict";

    // ----------------------------------------------------------
    // Back to top button
    // ----------------------------------------------------------
    const backToTop = document.getElementById("back-to-top");

    function updateBackToTop() {
        if (!backToTop) return;
        if (window.scrollY > 300) {
            backToTop.classList.add("is-visible");
        } else {
            backToTop.classList.remove("is-visible");
        }
    }

    if (backToTop) {
        backToTop.addEventListener("click", () => {
            window.scrollTo({ top: 0, behavior: "smooth" });
        });
    }

    window.addEventListener("scroll", updateBackToTop, { passive: true });
    updateBackToTop();

    // ----------------------------------------------------------
    // Sidebar active link tracking (IntersectionObserver)
    // ----------------------------------------------------------
    const sidebarLinks = Array.from(
        document.querySelectorAll(".sidebar-nav a[href^='#']")
    );

    if (sidebarLinks.length > 0) {
        const sectionIds = sidebarLinks.map((a) => a.getAttribute("href").slice(1));
        const sections = sectionIds
            .map((id) => document.getElementById(id))
            .filter(Boolean);

        let activeSectionId = null;

        const observer = new IntersectionObserver(
            (entries) => {
                entries.forEach((entry) => {
                    if (entry.isIntersecting) {
                        activeSectionId = entry.target.id;
                        sidebarLinks.forEach((link) => {
                            const isActive =
                                link.getAttribute("href") === "#" + activeSectionId;
                            link.classList.toggle("is-active", isActive);
                        });
                    }
                });
            },
            {
                rootMargin: "-64px 0px -60% 0px",
                threshold: 0,
            }
        );

        sections.forEach((s) => observer.observe(s));
    }

    // ----------------------------------------------------------
    // Smooth scroll for all in-page anchor links
    // ----------------------------------------------------------
    document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
        anchor.addEventListener("click", (e) => {
            const targetId = anchor.getAttribute("href").slice(1);
            const target = document.getElementById(targetId);
            if (target) {
                e.preventDefault();
                target.scrollIntoView({ behavior: "smooth", block: "start" });
                // update URL without reloading
                history.replaceState(null, "", "#" + targetId);
            }
        });
    });

})();
