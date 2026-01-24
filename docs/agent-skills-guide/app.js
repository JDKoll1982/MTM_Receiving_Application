/**
 * Agent Skills Guide - JavaScript Application
 * Last Updated: 2026-01-24
 * Features: Theme switching, smooth scrolling, localStorage persistence
 */

// ==========================================================================
// Theme Management
// ==========================================================================

class ThemeManager {
    constructor() {
        this.themeToggle = document.querySelector('.theme-toggle');
        this.themeIcon = document.querySelector('.theme-icon');
        this.currentTheme = this.loadTheme();
        this.init();
    }

    init() {
        this.applyTheme(this.currentTheme);
        
        if (this.themeToggle) {
            this.themeToggle.addEventListener('click', () => this.toggleTheme());
        }
    }

    loadTheme() {
        const saved = localStorage.getItem('theme');
        if (saved) {
            return saved;
        }
        
        // Check system preference
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return 'dark';
        }
        
        return 'light';
    }

    applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        
        if (this.themeIcon) {
            this.themeIcon.textContent = theme === 'dark' ? '☀️' : '🌙';
        }
        
        if (this.themeToggle) {
            this.themeToggle.setAttribute('aria-label', 
                theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'
            );
        }
    }

    toggleTheme() {
        this.currentTheme = this.currentTheme === 'light' ? 'dark' : 'light';
        this.applyTheme(this.currentTheme);
        localStorage.setItem('theme', this.currentTheme);
    }
}

// ==========================================================================
// Navigation - Active Link Highlighting
// ==========================================================================

class NavigationManager {
    constructor() {
        this.init();
    }

    init() {
        const currentPage = window.location.pathname.split('/').pop() || 'index.html';
        const navLinks = document.querySelectorAll('.main-nav a');
        
        navLinks.forEach(link => {
            const linkPage = link.getAttribute('href');
            
            if (linkPage === currentPage || (currentPage === '' && linkPage === 'index.html')) {
                link.classList.add('active');
                link.setAttribute('aria-current', 'page');
            } else {
                link.classList.remove('active');
                link.removeAttribute('aria-current');
            }
        });
    }
}

// ==========================================================================
// Smooth Scroll Enhancement
// ==========================================================================

class ScrollManager {
    constructor() {
        this.init();
    }

    init() {
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', (e) => {
                const href = anchor.getAttribute('href');
                
                if (href === '#') return;
                
                e.preventDefault();
                const target = document.querySelector(href);
                
                if (target) {
                    const headerOffset = 80;
                    const elementPosition = target.getBoundingClientRect().top;
                    const offsetPosition = elementPosition + window.pageYOffset - headerOffset;

                    window.scrollTo({
                        top: offsetPosition,
                        behavior: 'smooth'
                    });
                    
                    target.focus({ preventScroll: true });
                }
            });
        });
    }
}

// ==========================================================================
// Search Functionality (for future enhancement)
// ==========================================================================

class SearchManager {
    constructor() {
        this.searchInput = document.querySelector('.search-input');
        this.searchResults = document.querySelector('.search-results');
        this.init();
    }

    init() {
        if (!this.searchInput) return;
        
        this.searchInput.addEventListener('input', (e) => {
            this.handleSearch(e.target.value);
        });
    }

    handleSearch(query) {
        if (query.length < 2) {
            this.clearResults();
            return;
        }
        
        // Search implementation would go here
        // For now, this is a placeholder for future enhancement
        console.log('Searching for:', query);
    }

    clearResults() {
        if (this.searchResults) {
            this.searchResults.innerHTML = '';
        }
    }
}

// ==========================================================================
// Code Block Copy Functionality
// ==========================================================================

class CodeCopyManager {
    constructor() {
        this.init();
    }

    init() {
        document.querySelectorAll('pre code').forEach((codeBlock) => {
            const button = this.createCopyButton();
            
            const wrapper = document.createElement('div');
            wrapper.classList.add('code-wrapper');
            
            codeBlock.parentNode.insertBefore(wrapper, codeBlock);
            wrapper.appendChild(button);
            wrapper.appendChild(codeBlock.parentNode);
            
            button.addEventListener('click', () => this.copyCode(codeBlock, button));
        });
    }

    createCopyButton() {
        const button = document.createElement('button');
        button.classList.add('copy-button');
        button.textContent = 'Copy';
        button.setAttribute('aria-label', 'Copy code to clipboard');
        return button;
    }

    async copyCode(codeBlock, button) {
        const code = codeBlock.textContent;
        
        try {
            await navigator.clipboard.writeText(code);
            button.textContent = 'Copied!';
            button.classList.add('copied');
            
            setTimeout(() => {
                button.textContent = 'Copy';
                button.classList.remove('copied');
            }, 2000);
        } catch (err) {
            console.error('Failed to copy code:', err);
            button.textContent = 'Error';
            
            setTimeout(() => {
                button.textContent = 'Copy';
            }, 2000);
        }
    }
}

// ==========================================================================
// Table of Contents Generator
// ==========================================================================

class TableOfContentsManager {
    constructor() {
        this.tocContainer = document.querySelector('.table-of-contents');
        this.init();
    }

    init() {
        if (!this.tocContainer) return;
        
        const headings = document.querySelectorAll('main h2[id], main h3[id]');
        
        if (headings.length === 0) return;
        
        const nav = document.createElement('nav');
        nav.setAttribute('aria-label', 'Table of contents');
        
        const list = document.createElement('ul');
        
        headings.forEach(heading => {
            const li = document.createElement('li');
            const a = document.createElement('a');
            
            a.href = `#${heading.id}`;
            a.textContent = heading.textContent;
            
            if (heading.tagName === 'H3') {
                li.classList.add('toc-sub-item');
            }
            
            li.appendChild(a);
            list.appendChild(li);
        });
        
        nav.appendChild(list);
        this.tocContainer.appendChild(nav);
    }
}

// ==========================================================================
// Accessibility - Focus Visible Polyfill
// ==========================================================================

class FocusManager {
    constructor() {
        this.init();
    }

    init() {
        // Add keyboard navigation indicator
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Tab') {
                document.body.classList.add('user-is-tabbing');
            }
        });

        document.addEventListener('mousedown', () => {
            document.body.classList.remove('user-is-tabbing');
        });
    }
}

// ==========================================================================
// Initialize All Managers
// ==========================================================================

document.addEventListener('DOMContentLoaded', () => {
    new ThemeManager();
    new NavigationManager();
    new ScrollManager();
    new CodeCopyManager();
    new TableOfContentsManager();
    new FocusManager();
    
    console.log('✅ Agent Skills Guide loaded successfully');
});

// ==========================================================================
// System Preference Listener
// ==========================================================================

if (window.matchMedia) {
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
        if (!localStorage.getItem('theme')) {
            const theme = e.matches ? 'dark' : 'light';
            document.documentElement.setAttribute('data-theme', theme);
        }
    });
}
