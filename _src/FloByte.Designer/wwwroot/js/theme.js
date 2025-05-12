// Theme handling for FloByte
(function () {
    // Check for saved theme preference or use the system preference
    const getThemePreference = () => {
        if (typeof localStorage !== 'undefined' && localStorage.getItem('theme')) {
            return localStorage.getItem('theme');
        }
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    };

    // Apply the theme by adding or removing the 'dark' class on the html element
    const setTheme = (theme) => {
        const htmlElement = document.documentElement;
        if (theme === 'dark') {
            htmlElement.classList.add('dark');
        } else {
            htmlElement.classList.remove('dark');
        }
        localStorage.setItem('theme', theme);
    };

    // Set the theme when the page loads
    const theme = getThemePreference();
    setTheme(theme);

    // Make theme functions available globally
    window.themeManager = {
        getTheme: getThemePreference,
        setTheme: setTheme,
        toggleTheme: () => {
            const current = getThemePreference();
            const newTheme = current === 'dark' ? 'light' : 'dark';
            setTheme(newTheme);
            return newTheme;
        }
    };
})();
