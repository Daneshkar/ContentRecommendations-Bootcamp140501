(() => {
    const resultCover = document.querySelector('.result-cover');
    const coverImage = resultCover?.querySelector('img');
    coverImage?.addEventListener('error', () => resultCover.classList.add('image-failed'));

    const dialog = document.querySelector('#details-dialog');
    const openDetails = document.querySelector('#open-details');
    const closeDetails = document.querySelector('#close-details');

    openDetails?.addEventListener('click', () => {
        if (typeof dialog?.showModal === 'function') dialog.showModal();
    });

    closeDetails?.addEventListener('click', () => dialog?.close());
    dialog?.addEventListener('click', (event) => {
        if (event.target === dialog) dialog.close();
    });

    const nextForm = document.querySelector('#next-form');
    const loadingScreen = document.querySelector('#loading-screen');
    nextForm?.addEventListener('submit', () => {
        const button = nextForm.querySelector('button[type="submit"]');
        if (button) {
            button.disabled = true;
            button.classList.add('is-loading');
        }
        if (loadingScreen) loadingScreen.hidden = false;
    });
})();
