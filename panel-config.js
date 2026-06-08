/**
 * URL del panel Blazor en Railway.
 * Si Railway cambia de dominio, actualice solo esta línea.
 */
window.SUBLISPORT_PANEL_URL = 'https://sublisport-garcia-production.up.railway.app';

(function () {
  function panelLoginUrl() {
    var base = (window.SUBLISPORT_PANEL_URL || '').replace(/\/$/, '');
    var host = window.location.hostname;
    var isGitHubPages = host.endsWith('github.io') || host.endsWith('github.dev');
    if (isGitHubPages && base) {
      return base + '/login';
    }
    return '/login';
  }

  function wireLoginLinks() {
    document.querySelectorAll('[data-panel-login]').forEach(function (el) {
      el.href = panelLoginUrl();
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', wireLoginLinks);
  } else {
    wireLoginLinks();
  }
})();
