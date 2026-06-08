/**
 * Carga catálogo y formulario de cotización desde el panel (Railway).
 */
(function () {
  var DEFAULTS = {
    catalog: [
      { imageUrl: 'https://i.pinimg.com/736x/bf/c5/89/bfc589c46649a1c6978872470d1a8850.jpg', title: 'Real Madrid', subtitle: 'La Liga · España' },
      { imageUrl: 'https://i1-c.pinimg.com/1200x/8f/32/5a/8f325a52478201b47be4a79bbf12db1d.jpg', title: 'FC Barcelona', subtitle: 'La Liga · España' },
      { imageUrl: 'https://i.pinimg.com/736x/82/20/a4/8220a40857ff8088154d5a3c87ff5c51.jpg', title: 'Estilo Europeo', subtitle: 'Sublimado Premium' },
      { imageUrl: 'https://i1-c.pinimg.com/736x/ff/eb/1d/ffeb1db96f8bb8dc3728543407c493a5.jpg', title: 'Diseño Especial', subtitle: 'Full Color' },
      { imageUrl: 'https://i.pinimg.com/originals/cb/56/71/cb5671893cf9c11517b2f14441822d0d.png', title: 'Alianza Lima', subtitle: 'Liga 1 · Perú' },
      { imageUrl: 'https://i.pinimg.com/736x/ec/97/d9/ec97d997bd4a823f6076ad7cd5f6e2c2.jpg', title: 'Universitario', subtitle: 'Liga 1 · Perú' },
      { imageUrl: 'https://i1-c.pinimg.com/1200x/e9/59/34/e9593407ed67eef379b5153c46f02cbc.jpg', title: 'Sporting Cristal', subtitle: 'Liga 1 · Perú' },
      { imageUrl: 'https://i1-c.pinimg.com/1200x/fa/48/1a/fa481a192b539477798fc8614ca4a23b.jpg', title: 'Selección Perú', subtitle: 'Blanquirroja' }
    ],
    quote: {
      whatsAppPhone: '51960840874',
      responseNote: 'Respuesta en menos de 2 horas · La Victoria, Lima',
      quantityPlaceholder: 'Cantidad de prendas',
      namePlaceholder: 'Tu nombre o club',
      extraPlaceholder: 'Colores, diseño, fecha de entrega, escudo...',
      garments: [
        { label: 'Conjunto Completo', value: 'Conjunto completo (camiseta + short + medias)', iconClass: 'fas fa-tshirt' },
        { label: 'Solo Camiseta', value: 'Solo camiseta', iconClass: 'fas fa-circle-dot' },
        { label: 'Solo Short', value: 'Short deportivo', iconClass: 'fas fa-person-running' }
      ],
      sports: [
        { label: '⚽ Fútbol', value: 'Fútbol' },
        { label: '🏐 Vóley', value: 'Vóley' },
        { label: '🏀 Básquet', value: 'Básquet' },
        { label: '🚴 Ciclismo', value: 'Ciclismo' },
        { label: '🏅 Otro', value: 'Otro' }
      ],
      sizes: [
        { label: 'XS / S', value: 'XS / S' },
        { label: 'M / L', value: 'M / L' },
        { label: 'XL / XXL', value: 'XL / XXL' },
        { label: 'Tallas mixtas', value: 'Tallas mixtas' }
      ]
    }
  };

  var selectedPrenda = '';
  var selectedSport = '';
  var whatsAppPhone = DEFAULTS.quote.whatsAppPhone;

  function apiBase() {
    var base = (window.SUBLISPORT_PANEL_URL || '').replace(/\/$/, '');
    if (base) return base;
    if (window.location.hostname.endsWith('github.io') || window.location.hostname.endsWith('github.dev')) {
      return 'https://sublisport-garcia-production.up.railway.app';
    }
    return window.location.origin;
  }

  function esc(s) {
    return String(s || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/"/g, '&quot;');
  }

  function catalogCard(item) {
    return '<div class="jersey-card">' +
      '<img src="' + esc(item.imageUrl) + '" alt="' + esc(item.title) + '">' +
      '<div class="jersey-card-info"><h4>' + esc(item.title) + '</h4><p>' + esc(item.subtitle) + '</p></div>' +
      '</div>';
  }

  function renderCatalog(items) {
    var track = document.getElementById('catalogTrack');
    if (!track || !items || !items.length) return;
    var html = '';
    items.forEach(function (item) { html += catalogCard(item); });
    items.forEach(function (item) { html += catalogCard(item); });
    track.innerHTML = html;
    wireHover(track.querySelectorAll('.jersey-card'));
  }

  function renderGarments(garments) {
    var wrap = document.getElementById('garmentOpts');
    if (!wrap || !garments || !garments.length) return;
    wrap.innerHTML = garments.map(function (g, i) {
      return '<div class="prenda-opt' + (i === 0 ? ' active' : '') + '" data-value="' + esc(g.value) + '">' +
        '<i class="' + esc(g.iconClass || 'fas fa-tshirt') + '"></i><span>' + esc(g.label) + '</span></div>';
    }).join('');
    selectedPrenda = garments[0].value;
    wrap.querySelectorAll('.prenda-opt').forEach(function (el) {
      el.addEventListener('click', function () {
        wrap.querySelectorAll('.prenda-opt').forEach(function (d) { d.classList.remove('active'); });
        el.classList.add('active');
        selectedPrenda = el.getAttribute('data-value') || '';
      });
    });
    wireHover(wrap.querySelectorAll('.prenda-opt'));
  }

  function renderSports(sports) {
    var wrap = document.getElementById('sportTabs');
    if (!wrap || !sports || !sports.length) return;
    wrap.innerHTML = sports.map(function (s, i) {
      return '<div class="sport-tab' + (i === 0 ? ' active' : '') + '" data-value="' + esc(s.value) + '">' + esc(s.label) + '</div>';
    }).join('');
    selectedSport = sports[0].value;
    wrap.querySelectorAll('.sport-tab').forEach(function (el) {
      el.addEventListener('click', function () {
        wrap.querySelectorAll('.sport-tab').forEach(function (t) { t.classList.remove('active'); });
        el.classList.add('active');
        selectedSport = el.getAttribute('data-value') || '';
      });
    });
    wireHover(wrap.querySelectorAll('.sport-tab'));
  }

  function renderSizes(sizes) {
    var select = document.getElementById('sizeRange');
    if (!select || !sizes || !sizes.length) return;
    select.innerHTML = '<option value="" style="background:#111">Tallas...</option>' +
      sizes.map(function (s) {
        return '<option value="' + esc(s.value) + '" style="background:#111">' + esc(s.label) + '</option>';
      }).join('');
  }

  function applyQuoteTexts(quote) {
    if (!quote) return;
    whatsAppPhone = (quote.whatsAppPhone || DEFAULTS.quote.whatsAppPhone).replace(/\D/g, '');
    var qty = document.getElementById('quantity');
    var name = document.getElementById('clientName');
    var extra = document.getElementById('extraMsg');
    var note = document.getElementById('quoteResponseNote');
    if (qty && quote.quantityPlaceholder) qty.placeholder = quote.quantityPlaceholder;
    if (name && quote.namePlaceholder) name.placeholder = quote.namePlaceholder;
    if (extra && quote.extraPlaceholder) extra.placeholder = quote.extraPlaceholder;
    if (note && quote.responseNote) {
      note.innerHTML = '<i class="fas fa-clock" style="color:var(--gold)"></i> ' + esc(quote.responseNote);
    }
    document.querySelectorAll('[data-wa-contact]').forEach(function (a) {
      a.href = 'https://wa.me/' + whatsAppPhone;
    });
  }

  function wireHover(nodes) {
    var cursor = document.getElementById('cursor');
    var ring = document.getElementById('cursorRing');
    if (!cursor || !ring) return;
    nodes.forEach(function (el) {
      el.addEventListener('mouseenter', function () {
        cursor.style.transform = 'scale(2)';
        ring.style.transform = 'scale(1.4)';
        ring.style.opacity = '1';
      });
      el.addEventListener('mouseleave', function () {
        cursor.style.transform = 'scale(1)';
        ring.style.transform = 'scale(1)';
        ring.style.opacity = '0.6';
      });
    });
  }

  window.sendToWhatsApp = function () {
    var qty = document.getElementById('quantity');
    var size = document.getElementById('sizeRange');
    var name = document.getElementById('clientName');
    var extra = document.getElementById('extraMsg');
    var msg = '🏅 *COTIZACIÓN - SUBLISPORT GARCIA*\n\n';
    msg += '👕 *Prenda:* ' + (selectedPrenda || '—') + '\n';
    msg += '⚽ *Deporte:* ' + (selectedSport || '—') + '\n';
    if (qty && qty.value) msg += '📦 *Cantidad:* ' + qty.value + ' prendas\n';
    if (size && size.value) msg += '📏 *Tallas:* ' + size.value + '\n';
    if (name && name.value) msg += '👤 *Nombre/Club:* ' + name.value + '\n';
    if (extra && extra.value) msg += '📝 *Detalles:* ' + extra.value + '\n';
    msg += '\n¡Hola! Me gustaría recibir cotización. 😊';
    window.open('https://wa.me/' + whatsAppPhone + '?text=' + encodeURIComponent(msg), '_blank');
  };

  function applyConfig(data) {
    var catalog = (data && data.catalog && data.catalog.length) ? data.catalog : DEFAULTS.catalog;
    var quote = (data && data.quote) ? data.quote : DEFAULTS.quote;
    renderCatalog(catalog);
    renderGarments(quote.garments || DEFAULTS.quote.garments);
    renderSports(quote.sports || DEFAULTS.quote.sports);
    renderSizes(quote.sizes || DEFAULTS.quote.sizes);
    applyQuoteTexts(quote);
  }

  function load() {
    var url = apiBase() + '/api/landing-config';
    fetch(url)
      .then(function (r) { return r.ok ? r.json() : Promise.reject(); })
      .then(applyConfig)
      .catch(function () { applyConfig(DEFAULTS); });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', load);
  } else {
    load();
  }
})();
