(function () {
    "use strict";

    const body = document.body;
    const currentPage = body.getAttribute("data-mock-page");

    if (currentPage) {
        document
            .querySelectorAll("[data-mock-link]")
            .forEach((link) => {
                if (link.getAttribute("data-mock-link") === currentPage) {
                    link.classList.add("is-active");
                }
            });
    }
})();
