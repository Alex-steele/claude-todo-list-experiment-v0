window.todoApp = window.todoApp || {};

window.todoApp.registerKeyboardShortcuts = function (dotNetRef) {
    window._todoAppDotNetRef = dotNetRef;
    document.addEventListener('keydown', window.todoApp._handleKeyDown);
};

window.todoApp.unregisterKeyboardShortcuts = function () {
    document.removeEventListener('keydown', window.todoApp._handleKeyDown);
    window._todoAppDotNetRef = null;
};

window.todoApp.focusElement = function (selector) {
    const el = document.querySelector(selector);
    if (el) el.focus();
};

window.todoApp.clickElement = function (selector) {
    const el = document.querySelector(selector);
    if (el) el.click();
};

window.todoApp.downloadFile = function (filename, content, mimeType) {
    const blob = new Blob([content], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

window.todoApp.getDarkModePreference = function () {
    const stored = localStorage.getItem('todoApp:darkMode');
    if (stored !== null) return stored === 'true';
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
};

window.todoApp.saveDarkModePreference = function (isDark) {
    localStorage.setItem('todoApp:darkMode', isDark.toString());
};

// ── Drag-to-reorder ──────────────────────────────────────────────────────────
window.todoApp._drag = { fromId: null, dotNetRef: null };

window.todoApp.initDragDrop = function (dotNetRef) {
    window.todoApp._drag.dotNetRef = dotNetRef;
    document.removeEventListener('dragstart', window.todoApp._onDragStart);
    document.removeEventListener('dragover',  window.todoApp._onDragOver);
    document.removeEventListener('dragleave', window.todoApp._onDragLeave);
    document.removeEventListener('drop',      window.todoApp._onDrop);
    document.removeEventListener('dragend',   window.todoApp._onDragEnd);
    document.addEventListener('dragstart', window.todoApp._onDragStart);
    document.addEventListener('dragover',  window.todoApp._onDragOver);
    document.addEventListener('dragleave', window.todoApp._onDragLeave);
    document.addEventListener('drop',      window.todoApp._onDrop);
    document.addEventListener('dragend',   window.todoApp._onDragEnd);
};

window.todoApp.destroyDragDrop = function () {
    document.removeEventListener('dragstart', window.todoApp._onDragStart);
    document.removeEventListener('dragover',  window.todoApp._onDragOver);
    document.removeEventListener('dragleave', window.todoApp._onDragLeave);
    document.removeEventListener('drop',      window.todoApp._onDrop);
    document.removeEventListener('dragend',   window.todoApp._onDragEnd);
    window.todoApp._drag.dotNetRef = null;
};

window.todoApp._getDragItem = function (el) {
    while (el && !el.dataset.todoId) el = el.parentElement;
    return el;
};

window.todoApp._onDragStart = function (e) {
    const item = window.todoApp._getDragItem(e.target);
    if (!item) return;
    window.todoApp._drag.fromId = parseInt(item.dataset.todoId);
    e.dataTransfer.effectAllowed = 'move';
    item.classList.add('dragging');
};

window.todoApp._onDragOver = function (e) {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
    const item = window.todoApp._getDragItem(e.target);
    if (!item || !item.dataset.todoId) return;
    document.querySelectorAll('.drag-target').forEach(el => el.classList.remove('drag-target'));
    item.classList.add('drag-target');
};

window.todoApp._onDragLeave = function (e) {
    const item = window.todoApp._getDragItem(e.target);
    if (item) item.classList.remove('drag-target');
};

window.todoApp._onDrop = function (e) {
    e.preventDefault();
    const item = window.todoApp._getDragItem(e.target);
    if (!item || !item.dataset.todoId) return;
    const toId = parseInt(item.dataset.todoId);
    const fromId = window.todoApp._drag.fromId;
    document.querySelectorAll('.drag-target,.dragging').forEach(el => {
        el.classList.remove('drag-target');
        el.classList.remove('dragging');
    });
    if (fromId && toId && fromId !== toId) {
        window.todoApp._drag.dotNetRef?.invokeMethodAsync('OnDragDrop', fromId, toId);
    }
    window.todoApp._drag.fromId = null;
};

window.todoApp._onDragEnd = function (e) {
    document.querySelectorAll('.drag-target,.dragging').forEach(el => {
        el.classList.remove('drag-target');
        el.classList.remove('dragging');
    });
    window.todoApp._drag.fromId = null;
};

// ── Keyboard shortcuts ────────────────────────────────────────────────────────
window.todoApp._handleKeyDown = function (e) {
    const tag = e.target.tagName.toLowerCase();
    if (tag === 'input' || tag === 'textarea' || e.target.isContentEditable) return;

    if (e.key === 'n' || e.key === 'N') {
        e.preventDefault();
        window._todoAppDotNetRef?.invokeMethodAsync('FocusNewTodoInput');
    } else if (e.key === '/') {
        e.preventDefault();
        window._todoAppDotNetRef?.invokeMethodAsync('FocusSearchInput');
    } else if (e.key === '?') {
        e.preventDefault();
        window._todoAppDotNetRef?.invokeMethodAsync('ToggleShortcutsHelp');
    }
};
