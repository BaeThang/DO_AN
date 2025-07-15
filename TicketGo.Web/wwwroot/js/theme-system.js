/**
 * Theme System - Auto Dark/Light Mode Detection
 * This script provides automatic theme detection and manual override capabilities
 */

class ThemeSystem {
    constructor() {
        this.themeToggle = null;
        this.sunIcon = null;
        this.moonIcon = null;
        this.systemThemeQuery = window.matchMedia('(prefers-color-scheme: dark)');
        
        this.init();
    }

    init() {
        // Wait for DOM to be ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.setup());
        } else {
            this.setup();
        }
    }

    setup() {
        this.createThemeToggle();
        this.initializeTheme();
        this.setupEventListeners();
        this.addTransitionClass();
    }

    createThemeToggle() {
        // Check if theme toggle already exists
        if (document.getElementById('themeToggle')) {
            this.themeToggle = document.getElementById('themeToggle');
            this.sunIcon = document.getElementById('sunIcon');
            this.moonIcon = document.getElementById('moonIcon');
            return;
        }

        // Create theme toggle button
        const toggleButton = document.createElement('button');
        toggleButton.id = 'themeToggle';
        toggleButton.className = 'theme-toggle';
        toggleButton.setAttribute('aria-label', 'Toggle theme');
        toggleButton.title = 'Click to toggle theme, Double-click for auto mode';

        // Create sun icon
        const sunIcon = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
        sunIcon.id = 'sunIcon';
        sunIcon.setAttribute('viewBox', '0 0 24 24');
        sunIcon.style.display = 'block';
        sunIcon.innerHTML = '<path d="M12 2.25a.75.75 0 01.75.75v2.25a.75.75 0 01-1.5 0V3a.75.75 0 01.75-.75zM7.5 12a4.5 4.5 0 119 0 4.5 4.5 0 01-9 0zM18.894 6.166a.75.75 0 00-1.06-1.06l-1.591 1.59a.75.75 0 101.06 1.061l1.591-1.59zM21.75 12a.75.75 0 01-.75.75h-2.25a.75.75 0 010-1.5H21a.75.75 0 01.75.75zM17.834 18.894a.75.75 0 001.06-1.06l-1.59-1.591a.75.75 0 10-1.061 1.06l1.59 1.591zM12 18a.75.75 0 01.75.75V21a.75.75 0 01-1.5 0v-2.25A.75.75 0 0112 18zM7.758 17.303a.75.75 0 00-1.061-1.06l-1.591 1.59a.75.75 0 001.06 1.061l1.591-1.59zM6 12a.75.75 0 01-.75.75H3a.75.75 0 010-1.5h2.25A.75.75 0 016 12zM6.697 7.757a.75.75 0 001.06-1.06l-1.59-1.591a.75.75 0 00-1.061 1.06l1.59 1.591z"/>';

        // Create moon icon
        const moonIcon = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
        moonIcon.id = 'moonIcon';
        moonIcon.setAttribute('viewBox', '0 0 24 24');
        moonIcon.style.display = 'none';
        moonIcon.innerHTML = '<path d="M9.528 1.718a.75.75 0 01.162.819A8.97 8.97 0 009 6a9 9 0 009 9 8.97 8.97 0 003.463-.69.75.75 0 01.981.98 10.503 10.503 0 01-9.694 6.46c-5.799 0-10.5-4.701-10.5-10.5 0-4.368 2.667-8.112 6.46-9.694a.75.75 0 01.818.162z"/>';

        toggleButton.appendChild(sunIcon);
        toggleButton.appendChild(moonIcon);
        document.body.appendChild(toggleButton);

        this.themeToggle = toggleButton;
        this.sunIcon = sunIcon;
        this.moonIcon = moonIcon;
    }

    getSystemTheme() {
        return this.systemThemeQuery.matches ? 'dark' : 'light';
    }

    getEffectiveTheme() {
        const savedTheme = localStorage.getItem('theme');
        const systemTheme = this.getSystemTheme();
        
        // If no saved preference, use system theme
        if (!savedTheme) {
            return systemTheme;
        }
        
        // If saved preference is 'auto', use system theme
        if (savedTheme === 'auto') {
            return systemTheme;
        }
        
        // Otherwise use saved preference
        return savedTheme;
    }

    applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        this.updateThemeIcon(theme);
        
        // Dispatch custom event for other components
        document.dispatchEvent(new CustomEvent('themeChanged', { 
            detail: { theme } 
        }));
    }

    updateThemeIcon(theme) {
        if (!this.sunIcon || !this.moonIcon) return;
        
        if (theme === 'dark') {
            this.sunIcon.style.display = 'none';
            this.moonIcon.style.display = 'block';
        } else {
            this.sunIcon.style.display = 'block';
            this.moonIcon.style.display = 'none';
        }
    }

    initializeTheme() {
        const initialTheme = this.getEffectiveTheme();
        this.applyTheme(initialTheme);
        
        // Log current theme mode
        const savedTheme = localStorage.getItem('theme');
        if (!savedTheme || savedTheme === 'auto') {
            console.log('Theme mode: Auto (Following system preference)');
        } else {
            console.log('Theme mode: Manual (' + savedTheme + ')');
        }
    }

    setupEventListeners() {
        // System theme change listener
        this.systemThemeQuery.addEventListener('change', (e) => {
            const savedTheme = localStorage.getItem('theme');
            
            // Only auto-change if user hasn't set a specific preference or set to 'auto'
            if (!savedTheme || savedTheme === 'auto') {
                const newTheme = e.matches ? 'dark' : 'light';
                this.applyTheme(newTheme);
                this.showNotification('Theme tự động chuyển theo hệ thống');
            }
        });

        // Theme toggle button listeners
        if (this.themeToggle) {
            // Single click - toggle theme
            this.themeToggle.addEventListener('click', () => {
                const currentTheme = document.documentElement.getAttribute('data-theme');
                const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
                
                // Save user's explicit choice
                localStorage.setItem('theme', newTheme);
                this.applyTheme(newTheme);
                this.showNotification(`Chuyển sang ${newTheme === 'dark' ? 'Dark' : 'Light'} mode`);
            });

            // Double click - enable auto mode
            this.themeToggle.addEventListener('dblclick', () => {
                localStorage.setItem('theme', 'auto');
                const systemTheme = this.getSystemTheme();
                this.applyTheme(systemTheme);
                this.showNotification('Chế độ tự động đã bật - Theo dõi theme hệ thống');
            });
        }
    }

    addTransitionClass() {
        // Add transition class after initial load to avoid flash
        setTimeout(() => {
            document.body.classList.add('theme-transition');
        }, 100);
    }

    showNotification(message) {
        // Remove existing notification if any
        const existingNotification = document.querySelector('.theme-notification');
        if (existingNotification) {
            existingNotification.remove();
        }

        // Create notification element
        const notification = document.createElement('div');
        notification.className = 'theme-notification';
        notification.textContent = message;
        
        document.body.appendChild(notification);

        // Show notification
        setTimeout(() => {
            notification.classList.add('show');
        }, 100);

        // Hide and remove notification after 3 seconds
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.remove();
                }
            }, 300);
        }, 3000);
    }

    // Public methods
    setTheme(theme) {
        if (['light', 'dark', 'auto'].includes(theme)) {
            localStorage.setItem('theme', theme);
            
            if (theme === 'auto') {
                const systemTheme = this.getSystemTheme();
                this.applyTheme(systemTheme);
            } else {
                this.applyTheme(theme);
            }
        }
    }

    getCurrentTheme() {
        return document.documentElement.getAttribute('data-theme');
    }

    getThemeMode() {
        const savedTheme = localStorage.getItem('theme');
        return savedTheme || 'auto';
    }

    isSystemDark() {
        return this.systemThemeQuery.matches;
    }
}

// Initialize theme system when script loads
let themeSystem;

// Wait for DOM to be ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        themeSystem = new ThemeSystem();
    });
} else {
    themeSystem = new ThemeSystem();
}

// Export for use in other scripts
window.ThemeSystem = ThemeSystem;
window.themeSystem = themeSystem;
