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
