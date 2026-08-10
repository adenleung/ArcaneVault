(() => {
    const root = document.querySelector('[data-smart-add]');
    if (!root) return;
    const results = root.querySelector('[data-smart-results]');
    const message = root.querySelector('[data-smart-message]');
    const token = document.querySelector('form.editor-form input[name="__RequestVerificationToken"]')?.value;
    let uploadedImageUrl = '';

    const setMessage = (text, error = false) => {
        message.hidden = false; message.textContent = text;
        message.classList.toggle('error', error);
    };
    const escapeHtml = value => String(value ?? '').replace(/[&<>'"]/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#39;','"':'&quot;'}[c]));
    const apply = item => {
        const set = (id, value) => { const el = document.getElementById(id); if (el && value) el.value = value; };
        set('Input_ItemName', item.itemName || item.possibleName);
        set('Input_Description', item.description);
        set('Input_CategoryCode', item.categoryCode);
        set('Input_ImageUrl', uploadedImageUrl || item.imageUrl);
        setMessage('Details applied. Review every field, then add your purchase information before saving.');
        document.querySelector('.editor-form')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    };
    const render = (items, fallback) => {
        results.hidden = false;
        if (!items?.length) {
            results.innerHTML = `<div class="smart-empty"><strong>No verified catalogue match.</strong><p>You can apply the AI suggestion or complete the form manually.</p>${fallback ? '<button class="button button-light" type="button" data-fallback>Apply suggestion</button>' : ''}</div>`;
            results.querySelector('[data-fallback]')?.addEventListener('click', () => apply(fallback));
            return;
        }
        results.innerHTML = items.map((x, index) => `<article class="smart-match"><img src="${escapeHtml(x.imageUrl)}" alt="" /><div><span>Match ${index + 1}</span><h3>${escapeHtml(x.itemName)}</h3><p>${escapeHtml([x.brand, x.series, x.releaseYear].filter(Boolean).join(' · '))}</p><small>${escapeHtml(x.referenceNumber || 'No reference number')}</small></div><button class="button button-small" type="button" data-use="${index}">Use match</button></article>`).join('');
        results.querySelectorAll('[data-use]').forEach(button => button.addEventListener('click', () => apply(items[Number(button.dataset.use)])));
    };

    root.querySelector('[data-catalog-search]').addEventListener('click', async () => {
        const query = document.getElementById('catalogQuery').value.trim();
        if (query.length < 2) return setMessage('Enter at least two characters.', true);
        setMessage('Searching the verified catalogue…');
        try {
            const response = await fetch(`/Collection/Create?handler=CatalogSearch&query=${encodeURIComponent(query)}`);
            if (!response.ok) throw new Error();
            render(await response.json()); setMessage('Choose the closest verified match.');
        } catch { setMessage('Catalogue search is unavailable. You can still complete the form manually.', true); }
    });

    root.querySelector('[data-identify]').addEventListener('click', async () => {
        const file = document.getElementById('smartImage').files[0];
        if (!file) return setMessage('Choose an image first.', true);
        const form = new FormData(); form.append('image', file); form.append('__RequestVerificationToken', token || '');
        setMessage('Analysing visible details and checking the catalogue…'); results.hidden = true;
        try {
            const response = await fetch('/Collection/Create?handler=Identify', { method: 'POST', body: form });
            const data = await response.json();
            if (!response.ok) throw new Error(data.message || 'Identification failed.');
            uploadedImageUrl = data.uploadedImageUrl || '';
            const fallback = { ...data.identification, itemName: data.identification.possibleName };
            render(data.matches, fallback);
            setMessage(`${Math.round((data.identification.confidence || 0) * 100)}% identification confidence. Review before using a result.`);
        } catch (error) { setMessage(error.message || 'AI identification is unavailable. Search by name or enter the item manually.', true); }
    });
})();
