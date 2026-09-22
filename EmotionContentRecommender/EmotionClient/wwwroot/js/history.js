(() => {
    const cards = Array.from(document.querySelectorAll("[data-history-card]"));
    if (cards.length === 0) return;

    const list = document.getElementById("history-list");
    const search = document.getElementById("history-search");
    const rating = document.getElementById("history-rating");
    const sort = document.getElementById("history-sort");
    const count = document.getElementById("history-result-count");
    const empty = document.getElementById("history-filter-empty");
    const clear = document.getElementById("clear-history-filters");
    const loadMore = document.getElementById("history-load-more");
    const pageSize = 5;
    let visibleLimit = pageSize;

    const normalize = (value) => value.trim().toLocaleLowerCase("en-US");

    const applyFilters = (resetLimit = true) => {
        if (resetLimit) visibleLimit = pageSize;

        const query = normalize(search.value);
        const selectedRating = rating.value;
        const matching = cards.filter((card) => {
            const matchesSearch = !query || normalize(card.dataset.search || "").includes(query);
            const matchesRating = selectedRating === "all" || card.dataset.rating === selectedRating;
            return matchesSearch && matchesRating;
        });

        matching.sort((left, right) => {
            const leftDate = Date.parse(left.dataset.created || "") || 0;
            const rightDate = Date.parse(right.dataset.created || "") || 0;
            const leftRating = Number(left.dataset.rating || 0);
            const rightRating = Number(right.dataset.rating || 0);

            if (sort.value === "oldest") return leftDate - rightDate;
            if (sort.value === "highest") return rightRating - leftRating || rightDate - leftDate;
            if (sort.value === "lowest") return leftRating - rightRating || rightDate - leftDate;
            return rightDate - leftDate;
        });

        cards.forEach((card) => {
            card.hidden = true;
        });

        matching.forEach((card, index) => {
            list.appendChild(card);
            card.hidden = index >= visibleLimit;
        });

        const shown = Math.min(matching.length, visibleLimit);
        count.textContent = matching.length === 1 ? "1 experience" : `${matching.length} experiences`;
        empty.hidden = matching.length !== 0;
        loadMore.hidden = matching.length === 0 || shown >= matching.length;
    };

    search.addEventListener("input", () => applyFilters());
    rating.addEventListener("change", () => applyFilters());
    sort.addEventListener("change", () => applyFilters());
    loadMore.addEventListener("click", () => {
        visibleLimit += pageSize;
        applyFilters(false);
    });
    clear.addEventListener("click", () => {
        search.value = "";
        rating.value = "all";
        sort.value = "newest";
        search.focus();
        applyFilters();
    });

    document.querySelectorAll(".history-cover img").forEach((image) => {
        const markBroken = () => image.classList.add("is-broken");
        image.addEventListener("error", markBroken);
        if (image.complete && image.naturalWidth === 0) markBroken();
    });

    document.querySelectorAll("[data-local-time]").forEach((time) => {
        const date = new Date(time.dateTime);
        if (Number.isNaN(date.getTime())) return;

        time.textContent = new Intl.DateTimeFormat("en-US", {
            month: "short",
            day: "numeric",
            year: "numeric",
            hour: "numeric",
            minute: "2-digit"
        }).format(date);
    });

    document.querySelectorAll("[data-dialog-open]").forEach((button) => {
        button.addEventListener("click", () => {
            const dialog = document.getElementById(button.dataset.dialogOpen);
            if (!(dialog instanceof HTMLDialogElement)) return;
            dialog.showModal();
            document.body.classList.add("has-dialog-open");
        });
    });

    document.querySelectorAll(".experience-dialog").forEach((dialog) => {
        const closeDialog = () => {
            dialog.close();
            document.body.classList.remove("has-dialog-open");
        };

        dialog.querySelector("[data-dialog-close]")?.addEventListener("click", closeDialog);
        dialog.addEventListener("cancel", () => {
            document.body.classList.remove("has-dialog-open");
        });
        dialog.addEventListener("click", (event) => {
            if (event.target === dialog) closeDialog();
        });
    });

    applyFilters();
})();
