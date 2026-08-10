(() => {
    const root = document.querySelector('[data-vault-assistant]'); if (!root) return;
    const panel = root.querySelector('[data-assistant-panel]');
    const toggle = root.querySelector('[data-assistant-toggle]');
    const form = root.querySelector('[data-assistant-form]');
    const messages = root.querySelector('[data-assistant-messages]');
    const input = form.elements.question;
    const open = value => { panel.hidden = !value; toggle.setAttribute('aria-expanded', String(value)); if (value) input.focus(); };
    toggle.addEventListener('click', () => open(panel.hidden));
    root.querySelector('[data-assistant-close]').addEventListener('click', () => open(false));
    const add = (text, type) => { const item = document.createElement('div'); item.className = `assistant-message ${type}`; item.textContent = text; messages.appendChild(item); messages.scrollTop = messages.scrollHeight; return item; };
    const ask = async question => {
        if (!question.trim()) return;
        add(question, 'user'); input.value = ''; input.disabled = true;
        const waiting = add('Checking your collection…', 'bot waiting');
        try {
            const response = await fetch('/Assistant?handler=Ask', {
                method: 'POST', headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || '' },
                body: JSON.stringify({ question })
            });
            const data = await response.json(); if (!response.ok) throw new Error(data.message || 'Unable to answer.');
            waiting.textContent = data.answer;
        } catch (error) { waiting.textContent = error.message || 'Vault Assistant is temporarily unavailable.'; }
        finally { waiting.classList.remove('waiting'); input.disabled = false; input.focus(); }
    };
    form.addEventListener('submit', event => { event.preventDefault(); ask(input.value); });
    root.querySelectorAll('.assistant-suggestions button').forEach(button => button.addEventListener('click', () => ask(button.textContent)));
})();
