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

window.todoApp.copyToClipboard = function (text) {
    return navigator.clipboard.writeText(text);
};

window.todoApp.getReminderPreference = function () {
    return localStorage.getItem('todoApp:remindersEnabled') === 'true';
};

window.todoApp.saveReminderPreference = function (enabled) {
    localStorage.setItem('todoApp:remindersEnabled', enabled.toString());
};

window.todoApp.requestNotificationPermission = async function () {
    if (!('Notification' in window)) return 'unsupported';
    if (Notification.permission === 'granted') return 'granted';
    if (Notification.permission === 'denied') return 'denied';
    return await Notification.requestPermission();
};

window.todoApp.showNotification = function (title, body) {
    if ('Notification' in window && Notification.permission === 'granted') {
        new Notification(title, { body: body });
    }
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
