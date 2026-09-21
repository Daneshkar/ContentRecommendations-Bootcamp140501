(() => {
    const form = document.querySelector('#mood-recommendation-form');
    if (!form) return;

    const categoryInputs = [...form.querySelectorAll('input[name="ItemTypeId"]')];
    const moodInputs = [...form.querySelectorAll('.mood-selection-input')];
    const primaryMoodInput = form.querySelector('#primary-mood-id');
    const additionalMoodInputs = form.querySelector('#additional-mood-inputs');
    const themeInputs = [...form.querySelectorAll('input[name="ThemeIds"]')];
    const categoryError = document.querySelector('#category-error');
    const moodError = document.querySelector('#mood-error');
    const themeError = document.querySelector('#theme-error');
    const summaryCategory = document.querySelector('#summary-category');
    const summaryMood = document.querySelector('#summary-mood');
    const summaryAdditionalMoods = document.querySelector('#summary-additional-moods');
    const summaryThemes = document.querySelector('#summary-themes');
    const selectedThemes = document.querySelector('#selected-themes');
    const themeCount = document.querySelector('#theme-count');
    const themeTriggerLabel = document.querySelector('#theme-trigger-label');
    const themePicker = document.querySelector('#theme-picker');
    const themeTrigger = document.querySelector('#theme-trigger');
    const themeMenu = document.querySelector('#theme-menu');
    const themeSearch = document.querySelector('#theme-search');
    const moodSearch = document.querySelector('#mood-search');
    const moodGrid = document.querySelector('#mood-options');
    const browseMoods = document.querySelector('#browse-moods');
    const moodSearchEmpty = document.querySelector('#mood-search-empty');
    const clearButton = document.querySelector('#clear-selections');
    const submitButton = document.querySelector('#find-match-button');
    const loadingScreen = document.querySelector('#loading-screen');
    let selectedMoodIds = [
        Number(primaryMoodInput?.value || 0),
        ...[...additionalMoodInputs.querySelectorAll('input')].map((input) => Number(input.value))
    ].filter((id, index, values) => id > 0 && values.indexOf(id) === index);

    const checked = (inputs) => inputs.find((input) => input.checked);
    const checkedThemes = () => themeInputs.filter((input) => input.checked);

    const updateSummary = () => {
        const category = checked(categoryInputs);
        const themes = checkedThemes();
        const selectedMoods = selectedMoodIds
            .map((id) => moodInputs.find((input) => Number(input.value) === id))
            .filter(Boolean);

        summaryCategory.textContent = category?.dataset.name || 'Not selected';
        summaryMood.textContent = selectedMoods[0]?.dataset.name || 'Not selected';
        summaryAdditionalMoods.textContent = selectedMoods.length > 1
            ? selectedMoods.slice(1).map((input) => input.dataset.name).join(', ')
            : 'None';
        summaryThemes.textContent = themes.length === 0
            ? 'Any'
            : themes.length === 1
                ? themes[0].dataset.name
                : `${themes.length} selected`;

        themeCount.textContent = String(themes.length);
        themeTriggerLabel.textContent = themes.length === 0
            ? 'Any theme'
            : themes.length === 1
                ? themes[0].dataset.name
                : `${themes.length} themes selected`;

        renderSelectedThemes(themes);
    };

    const syncMoodSelection = () => {
        primaryMoodInput.value = selectedMoodIds[0] || 0;
        additionalMoodInputs.replaceChildren();

        selectedMoodIds.slice(1).forEach((id) => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = 'AdditionalMoodIds';
            input.value = String(id);
            additionalMoodInputs.append(input);
        });

        moodInputs.forEach((input) => {
            const position = selectedMoodIds.indexOf(Number(input.value));
            input.checked = position >= 0;
            const role = input.closest('.mood-chip')?.querySelector('[data-mood-role]');
            if (role) role.textContent = position === 0 ? 'Primary' : position > 0 ? 'Additional' : '';
        });

        updateSummary();
    };

    const renderSelectedThemes = (themes) => {
        selectedThemes.replaceChildren();

        themes.forEach((input) => {
            const chip = document.createElement('span');
            chip.className = 'selected-theme';
            chip.append(document.createTextNode(input.dataset.name || 'Theme'));

            const remove = document.createElement('button');
            remove.type = 'button';
            remove.setAttribute('aria-label', `Remove ${input.dataset.name || 'theme'}`);
            remove.textContent = '×';
            remove.addEventListener('click', () => {
                input.checked = false;
                themeError.textContent = '';
                updateSummary();
            });

            chip.append(remove);
            selectedThemes.append(chip);
        });
    };

    categoryInputs.forEach((input) => input.addEventListener('change', () => {
        categoryError.textContent = '';
        updateSummary();
    }));

    moodInputs.forEach((input) => input.addEventListener('change', () => {
        const moodId = Number(input.value);
        if (input.checked) {
            if (selectedMoodIds.length >= 3) {
                input.checked = false;
                moodError.textContent = 'Choose no more than three moods.';
                return;
            }
            selectedMoodIds.push(moodId);
        } else {
            selectedMoodIds = selectedMoodIds.filter((id) => id !== moodId);
        }

        moodError.textContent = '';
        syncMoodSelection();
    }));

    themeInputs.forEach((input) => input.addEventListener('change', () => {
        const themes = checkedThemes();
        if (themes.length > 5) {
            input.checked = false;
            themeError.textContent = 'Choose no more than five themes.';
        } else {
            themeError.textContent = '';
        }
        updateSummary();
    }));

    themeTrigger?.addEventListener('click', () => {
        const isOpen = themeTrigger.getAttribute('aria-expanded') === 'true';
        themeTrigger.setAttribute('aria-expanded', String(!isOpen));
        themeMenu.hidden = isOpen;
        if (!isOpen) themeSearch?.focus();
    });

    document.addEventListener('click', (event) => {
        if (!themePicker?.contains(event.target) && themeMenu && !themeMenu.hidden) {
            themeMenu.hidden = true;
            themeTrigger?.setAttribute('aria-expanded', 'false');
        }
    });

    themeSearch?.addEventListener('input', () => {
        const term = themeSearch.value.trim().toLowerCase();
        document.querySelectorAll('#theme-options > label').forEach((option) => {
            option.classList.toggle('is-filtered', !option.dataset.themeName.includes(term));
        });
    });

    browseMoods?.addEventListener('click', () => {
        const isExpanded = moodGrid.classList.toggle('is-expanded');
        browseMoods.setAttribute('aria-expanded', String(isExpanded));
        browseMoods.firstChild.textContent = isExpanded
            ? 'Show popular moods '
            : `Browse all ${moodInputs.length} moods `;
    });

    moodSearch?.addEventListener('input', () => {
        const term = moodSearch.value.trim().toLowerCase();
        const isSearching = term.length > 0;
        let visibleCount = 0;

        moodGrid.classList.toggle('is-searching', isSearching);
        document.querySelectorAll('#mood-options .mood-chip').forEach((option) => {
            const isFiltered = isSearching && !option.dataset.moodName.includes(term);
            option.classList.toggle('is-filtered', isFiltered);
            if (!isFiltered) visibleCount += 1;
        });

        moodSearchEmpty.hidden = !isSearching || visibleCount > 0;
        browseMoods.hidden = isSearching;
    });

    clearButton?.addEventListener('click', () => {
        [...categoryInputs, ...moodInputs, ...themeInputs].forEach((input) => {
            input.checked = false;
        });
        selectedMoodIds = [];
        syncMoodSelection();
        categoryError.textContent = '';
        moodError.textContent = '';
        themeError.textContent = '';
        moodSearch.value = '';
        moodGrid.classList.remove('is-searching');
        document.querySelectorAll('#mood-options .mood-chip').forEach((option) => {
            option.classList.remove('is-filtered');
        });
        browseMoods.hidden = false;
        moodSearchEmpty.hidden = true;
        updateSummary();
        document.querySelector('#category-heading')?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    });

    form.addEventListener('submit', (event) => {
        let firstInvalid = null;

        if (!checked(categoryInputs)) {
            categoryError.textContent = 'Choose a category to continue.';
            firstInvalid = document.querySelector('#category-options');
        }

        if (selectedMoodIds.length === 0) {
            moodError.textContent = 'Choose at least one mood to continue.';
            firstInvalid ||= document.querySelector('#mood-options');
        }

        if (checkedThemes().length > 5) {
            themeError.textContent = 'Choose no more than five themes.';
            firstInvalid ||= themePicker;
        }

        if (firstInvalid) {
            event.preventDefault();
            firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
            return;
        }

        submitButton.disabled = true;
        submitButton.classList.add('is-loading');
        loadingScreen.hidden = false;
    });

    syncMoodSelection();
})();
