(() => {
    const form = document.querySelector('#experience-start-form');
    if (!form) return;

    const cards = [...form.querySelectorAll('.media-choice-card')];
    const inputs = cards.map((card) => card.querySelector('input[type="radio"]'));
    const search = document.querySelector('#media-search');
    const filters = [...document.querySelectorAll('[data-category-filter]')];
    const grid = document.querySelector('#media-choice-grid');
    const browseButton = document.querySelector('#browse-media');
    const emptyMessage = document.querySelector('#catalog-empty-message');
    const count = document.querySelector('#catalog-count');
    const error = document.querySelector('#media-error');
    const submit = document.querySelector('#share-experience-button');
    const summaryCover = document.querySelector('#experience-summary-cover');
    const summaryTitle = document.querySelector('#experience-summary-title');
    const summaryMeta = document.querySelector('#experience-summary-meta');
    const loading = document.querySelector('#loading-screen');
    let activeCategory = 'all';
    let expanded = false;

    const updateFilters = () => {
        const term = search.value.trim().toLowerCase();
        let visible = 0;

        cards.forEach((card, index) => {
            const matchesSearch = card.dataset.mediaName.includes(term);
            const matchesCategory = activeCategory === 'all' || card.dataset.categoryId === activeCategory;
            const withinInitialSet = expanded || term.length > 0 || activeCategory !== 'all' || index < 12;
            const show = matchesSearch && matchesCategory && withinInitialSet;
            card.hidden = !show;
            if (show) visible += 1;
        });

        count.textContent = `${visible} ${visible === 1 ? 'title' : 'titles'}`;
        emptyMessage.hidden = visible > 0;
        if (browseButton) {
            const canBrowse = term.length === 0 && activeCategory === 'all' && cards.length > 12;
            browseButton.hidden = !canBrowse;
        }
    };

    const updateSummary = (input) => {
        summaryTitle.textContent = input.dataset.title;
        summaryMeta.textContent = [input.dataset.category, input.dataset.year].filter(Boolean).join(' · ');
        submit.disabled = false;
        error.textContent = '';

        const oldImage = summaryCover.querySelector('img');
        oldImage?.remove();
        summaryCover.classList.remove('has-media');
        if (input.dataset.cover) {
            const image = document.createElement('img');
            image.src = input.dataset.cover;
            image.alt = '';
            image.referrerPolicy = 'no-referrer';
            image.addEventListener('load', () => summaryCover.classList.add('has-media'));
            image.addEventListener('error', () => image.remove());
            summaryCover.prepend(image);
        }
    };

    inputs.forEach((input) => input.addEventListener('change', () => {
        if (input.checked) updateSummary(input);
    }));

    filters.forEach((button) => button.addEventListener('click', () => {
        activeCategory = button.dataset.categoryFilter;
        filters.forEach((candidate) => candidate.classList.toggle('is-active', candidate === button));
        updateFilters();
    }));

    search.addEventListener('input', updateFilters);

    browseButton?.addEventListener('click', () => {
        expanded = !expanded;
        grid.classList.toggle('is-expanded', expanded);
        browseButton.setAttribute('aria-expanded', String(expanded));
        browseButton.firstChild.textContent = expanded ? 'Show fewer titles ' : 'Browse all titles ';
        updateFilters();
    });

    form.addEventListener('submit', (event) => {
        const selected = inputs.find((input) => input.checked);
        if (!selected) {
            event.preventDefault();
            error.textContent = 'Choose a title to continue.';
            grid.scrollIntoView({ behavior: 'smooth', block: 'center' });
            return;
        }

        submit.disabled = true;
        submit.classList.add('is-loading');
        loading.hidden = false;
    });

    cards.forEach((card) => {
        const image = card.querySelector('img');
        image?.addEventListener('load', () => card.querySelector('.media-choice-cover')?.classList.add('has-media'));
        image?.addEventListener('error', () => image.remove());
    });

    const initiallySelected = inputs.find((input) => input.checked);
    if (initiallySelected) updateSummary(initiallySelected);
    updateFilters();
})();
