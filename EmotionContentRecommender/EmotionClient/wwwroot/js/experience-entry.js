(() => {
    const form = document.querySelector('#experience-entry-form');
    if (!form) return;

    const ratings = [...form.querySelectorAll('input[name="Score"]')];
    const moods = [...form.querySelectorAll('input[name="MoodIds"]')];
    const themes = [...form.querySelectorAll('input[name="ThemeIds"]')];
    const ratingError = document.querySelector('#rating-error');
    const moodError = document.querySelector('#experience-mood-error');
    const themeError = document.querySelector('#experience-theme-error');
    const ratingSummary = document.querySelector('#entry-summary-rating');
    const moodSummary = document.querySelector('#entry-summary-moods');
    const themeSummary = document.querySelector('#entry-summary-themes');
    const submit = document.querySelector('#find-experience-match-button');
    const submitLabel = document.querySelector('#experience-submit-label');
    const loading = document.querySelector('#loading-screen');
    const loadingMessage = document.querySelector('#experience-loading-message');

    const selected = (inputs) => inputs.filter((input) => input.checked);

    const updateSummary = () => {
        const rating = ratings.find((input) => input.checked);
        const selectedMoods = selected(moods);
        const selectedThemes = selected(themes);
        const score = Number(rating?.value || 0);
        ratingSummary.textContent = rating ? `${rating.value} / 5` : 'Not selected';
        moodSummary.textContent = selectedMoods.length === 0
            ? 'Not selected'
            : selectedMoods.map((input) => input.dataset.name).join(', ');
        themeSummary.textContent = selectedThemes.length === 0
            ? 'Any'
            : selectedThemes.map((input) => input.dataset.name).join(', ');
        submitLabel.textContent = score >= 4
            ? 'Find my next match'
            : 'Save my experience';
    };

    ratings.forEach((input) => input.addEventListener('change', () => {
        ratingError.textContent = '';
        updateSummary();
    }));

    moods.forEach((input) => input.addEventListener('change', () => {
        if (selected(moods).length > 3) {
            input.checked = false;
            moodError.textContent = 'Choose no more than three moods.';
        } else {
            moodError.textContent = '';
        }
        updateSummary();
    }));

    themes.forEach((input) => input.addEventListener('change', () => {
        if (selected(themes).length > 5) {
            input.checked = false;
            themeError.textContent = 'Choose no more than five themes.';
        } else {
            themeError.textContent = '';
        }
        updateSummary();
    }));

    const connectSearch = (searchId, containerId) => {
        const search = document.querySelector(searchId);
        const container = document.querySelector(containerId);
        if (!search || !container) return;

        search.addEventListener('input', () => {
            const term = search.value.trim().toLowerCase();
            container.classList.toggle('is-searching', term.length > 0);
            container.querySelectorAll('.experience-chip').forEach((chip) => {
                chip.classList.toggle('is-filtered', term.length > 0 && !chip.dataset.name.includes(term));
            });
        });
    };

    connectSearch('#experience-mood-search', '#experience-mood-options');
    connectSearch('#experience-theme-search', '#experience-theme-options');

    document.querySelectorAll('[data-browse-target]').forEach((button) => {
        button.addEventListener('click', () => {
            const target = document.querySelector(`#${button.dataset.browseTarget}`);
            const expanded = target.classList.toggle('is-expanded');
            button.setAttribute('aria-expanded', String(expanded));
            const noun = button.dataset.browseTarget.includes('mood') ? 'moods' : 'themes';
            button.firstChild.textContent = expanded ? `Show fewer ${noun} ` : `Browse all ${noun} `;
        });
    });

    form.addEventListener('submit', (event) => {
        let firstInvalid = null;
        const rating = ratings.find((input) => input.checked);
        const selectedMoods = selected(moods);
        const selectedThemes = selected(themes);

        if (!rating) {
            ratingError.textContent = 'Choose a rating between 1 and 5.';
            firstInvalid = document.querySelector('#rating-heading');
        }
        if (selectedMoods.length === 0 || selectedMoods.length > 3) {
            moodError.textContent = 'Choose between one and three moods.';
            firstInvalid ||= document.querySelector('#experience-moods-heading');
        }
        if (selectedThemes.length > 5) {
            themeError.textContent = 'Choose no more than five themes.';
            firstInvalid ||= document.querySelector('#experience-themes-heading');
        }

        if (firstInvalid) {
            event.preventDefault();
            firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
            return;
        }

        submit.disabled = true;
        submit.classList.add('is-loading');
        loadingMessage.textContent = Number(rating.value) >= 4
            ? 'Finding something with a similar emotional tone.'
            : 'Adding your feedback to WatchPair.';
        loading.hidden = false;
    });

    const cover = document.querySelector('.entry-summary .experience-summary-cover');
    const coverImage = cover?.querySelector('img');
    coverImage?.addEventListener('load', () => cover.classList.add('has-media'));
    coverImage?.addEventListener('error', () => coverImage.remove());
    updateSummary();
})();
