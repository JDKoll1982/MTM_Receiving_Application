
(function () {
    "use strict";

    // ----------------------------------------------------------
    // Back-to-top button
    // ----------------------------------------------------------
    const backToTop = document.getElementById("back-to-top");

    function updateBackToTop() {
        if (!backToTop) return;
        backToTop.classList.toggle("is-visible", window.scrollY > 300);
    }

    if (backToTop) {
        backToTop.addEventListener("click", () =>
            window.scrollTo({ top: 0, behavior: "smooth" })
        );
    }
    window.addEventListener("scroll", updateBackToTop, { passive: true });
    updateBackToTop();

    // ----------------------------------------------------------
    // Sidebar active-link tracking (IntersectionObserver)
    // ----------------------------------------------------------
    const sidebarLinks = Array.from(
        document.querySelectorAll(".sidebar-nav a[href^='#']")
    );

    if (sidebarLinks.length > 0) {
        const sections = sidebarLinks
            .map((a) => document.getElementById(a.getAttribute("href").slice(1)))
            .filter(Boolean);

        const observer = new IntersectionObserver(
            (entries) => {
                entries.forEach((entry) => {
                    if (entry.isIntersecting) {
                        const id = entry.target.id;
                        sidebarLinks.forEach((link) =>
                            link.classList.toggle(
                                "is-active",
                                link.getAttribute("href") === "#" + id
                            )
                        );
                    }
                });
            },
            { rootMargin: "-64px 0px -60% 0px", threshold: 0 }
        );
        sections.forEach((s) => observer.observe(s));
    }

    // ----------------------------------------------------------
    // Smooth scroll for in-page anchors
    // ----------------------------------------------------------
    document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
        anchor.addEventListener("click", (e) => {
            const target = document.getElementById(
                anchor.getAttribute("href").slice(1)
            );
            if (target) {
                e.preventDefault();
                target.scrollIntoView({ behavior: "smooth", block: "start" });
                history.pushState(null, "", anchor.getAttribute("href"));
            }
        });
    });

    // ----------------------------------------------------------
    // Lightbox for screenshots
    // ----------------------------------------------------------
    const overlay   = document.getElementById("lightbox-overlay");
    const lbImg     = document.getElementById("lightbox-img");
    const lbCaption = document.getElementById("lightbox-caption");
    const lbClose   = document.getElementById("lightbox-close");

    function openLightbox(src, caption) {
        if (!overlay || !lbImg) return;
        lbImg.src = src;
        lbImg.alt = caption || "";
        if (lbCaption) lbCaption.textContent = caption || "";
        overlay.classList.add("is-open");
        document.body.style.overflow = "hidden";
        lbClose && lbClose.focus();
    }

    function closeLightbox() {
        if (!overlay) return;
        overlay.classList.remove("is-open");
        document.body.style.overflow = "";
        lbImg.src = "";
    }

    // Attach click handler to all .screenshot-wrap and .screenshot-inline
    document.querySelectorAll(".screenshot-wrap, .screenshot-inline").forEach((wrap) => {
        wrap.setAttribute("role", "button");
        wrap.setAttribute("tabindex", "0");
        const img = wrap.querySelector("img");
        if (!img) return;

        const caption =
            wrap.dataset.caption ||
            img.alt ||
            wrap.nextElementSibling?.classList.contains("screenshot-caption")
                ? wrap.nextElementSibling?.textContent
                : "";

        function trigger() { openLightbox(img.src, caption); }

        wrap.addEventListener("click", trigger);
        wrap.addEventListener("keydown", (e) => {
            if (e.key === "Enter" || e.key === " ") { e.preventDefault(); trigger(); }
        });
    });

    // Close handlers
    if (overlay) {
        overlay.addEventListener("click", (e) => {
            if (e.target === overlay) closeLightbox();
        });
    }
    if (lbClose) {
        lbClose.addEventListener("click", closeLightbox);
    }
    document.addEventListener("keydown", (e) => {
        if (e.key === "Escape") closeLightbox();
    });

})();
