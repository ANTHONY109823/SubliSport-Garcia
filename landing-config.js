/**
 * Catálogo y cotización avanzada (lista jugadores, foto referencia, panel + WhatsApp).
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
      namePlaceholder: 'Tu nombre o club *',
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
  var rosterRows = [{ name: '', size: '', number: '' }];
  var referenceImageBase64 = null;
  var lastPanelResult = null;
  var isSubmitting = false;

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

  function $(id) { return document.getElementById(id); }

  function setStatus(msg, type) {
    var el = $('quoteStatus');
    if (!el) return;
    el.textContent = msg || '';
    el.className = 'quote-status' + (type ? ' ' + type : '');
  }

  function catalogCard(item) {
    return '<div class="jersey-card">' +
      '<img src="' + esc(item.imageUrl) + '" alt="' + esc(item.title) + '">' +
      '<div class="jersey-card-info"><h4>' + esc(item.title) + '</h4><p>' + esc(item.subtitle) + '</p></div>' +
      '</div>';
  }

  function renderCatalog(items) {
    var track = $('catalogTrack');
    if (!track || !items || !items.length) return;
    var html = '';
    items.forEach(function (item) { html += catalogCard(item); });
    items.forEach(function (item) { html += catalogCard(item); });
    track.innerHTML = html;
    wireHover(track.querySelectorAll('.jersey-card'));
  }

  function renderGarments(garments) {
    var wrap = $('garmentOpts');
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
    var wrap = $('sportTabs');
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
    var select = $('sizeRange');
    if (!select || !sizes || !sizes.length) return;
    select.innerHTML = '<option value="" style="background:#111">Rango de tallas general...</option>' +
      sizes.map(function (s) {
        return '<option value="' + esc(s.value) + '" style="background:#111">' + esc(s.label) + '</option>';
      }).join('');
  }

  function renderRoster() {
    var body = $('rosterBody');
    if (!body) return;
    body.innerHTML = rosterRows.map(function (row, i) {
      return '<tr data-idx="' + i + '">' +
        '<td><input type="text" data-field="name" value="' + esc(row.name) + '" placeholder="Nombre"></td>' +
        '<td><input type="text" data-field="size" value="' + esc(row.size) + '" placeholder="M"></td>' +
        '<td><input type="text" data-field="number" value="' + esc(row.number) + '" placeholder="10"></td>' +
        '<td>' + (rosterRows.length > 1 ? '<button type="button" class="btn-roster-del" data-remove="' + i + '">✕</button>' : '') + '</td>' +
        '</tr>';
    }).join('');

    body.querySelectorAll('input').forEach(function (input) {
      input.addEventListener('input', function () {
        var tr = input.closest('tr');
        var idx = parseInt(tr.getAttribute('data-idx'), 10);
        var field = input.getAttribute('data-field');
        if (!isNaN(idx) && rosterRows[idx]) rosterRows[idx][field] = input.value;
      });
    });

    body.querySelectorAll('[data-remove]').forEach(function (btn) {
      btn.addEventListener('click', function () {
        var idx = parseInt(btn.getAttribute('data-remove'), 10);
        if (rosterRows.length > 1) {
          rosterRows.splice(idx, 1);
          renderRoster();
        }
      });
    });
  }

  function addRosterRow() {
    rosterRows.push({ name: '', size: '', number: '' });
    renderRoster();
  }

  function getRosterFromDom() {
    return rosterRows
      .map(function (r) {
        return { name: (r.name || '').trim(), size: (r.size || '').trim(), number: (r.number || '').trim() };
      })
      .filter(function (r) { return r.name || r.size || r.number; });
  }

  function compressImage(file, callback) {
    var reader = new FileReader();
    reader.onload = function (e) {
      var img = new Image();
      img.onload = function () {
        var max = 1400;
        var w = img.width;
        var h = img.height;
        if (w > max || h > max) {
          if (w > h) { h = Math.round(h * max / w); w = max; }
          else { w = Math.round(w * max / h); h = max; }
        }
        var canvas = document.createElement('canvas');
        canvas.width = w;
        canvas.height = h;
        canvas.getContext('2d').drawImage(img, 0, 0, w, h);
        var dataUrl = canvas.toDataURL('image/jpeg', 0.82);
        callback(dataUrl);
      };
      img.src = e.target.result;
    };
    reader.readAsDataURL(file);
  }

  function setupPhotoUpload() {
    var input = $('referencePhoto');
    var preview = $('photoPreview');
    var zone = $('photoZone');
    if (!input) return;

    input.addEventListener('change', function () {
      var file = input.files && input.files[0];
      if (!file) return;
      if (file.size > 4 * 1024 * 1024) {
        setStatus('La imagen supera 4 MB. Elija otra.', 'err');
        input.value = '';
        return;
      }
      compressImage(file, function (dataUrl) {
        referenceImageBase64 = dataUrl;
        if (preview) {
          preview.src = dataUrl;
          preview.classList.add('visible');
        }
        if (zone) zone.classList.add('has-file');
        setStatus('Foto lista para enviar.', 'ok');
      });
    });
  }

  function collectForm() {
    var qty = $('quantity');
    var size = $('sizeRange');
    var name = $('clientName');
    var phone = $('clientPhone');
    var extra = $('extraMsg');
    var roster = getRosterFromDom();

    if (!name || !name.value.trim()) {
      setStatus('Indique su nombre o club.', 'err');
      name && name.focus();
      return null;
    }

    return {
      clientName: name.value.trim(),
      clientPhone: phone && phone.value ? phone.value.trim() : '',
      garmentType: selectedPrenda,
      sport: selectedSport,
      quantity: qty && qty.value ? parseInt(qty.value, 10) || 1 : Math.max(1, roster.length),
      sizeRangeSummary: size && size.value ? size.value : '',
      notes: extra && extra.value ? extra.value.trim() : '',
      roster: roster,
      referenceImageBase64: referenceImageBase64
    };
  }

  function formatRosterText(roster) {
    if (!roster.length) return '';
    var lines = roster.map(function (r, i) {
      return (i + 1) + '. ' + r.name + ' · Talla ' + r.size + ' · N°' + r.number;
    });
    return '\n👥 *Lista jugadores:*\n' + lines.join('\n') + '\n';
  }

  function buildWhatsAppMessage(data, panelInfo) {
    var msg = '🏅 *COTIZACIÓN - SUBLISPORT GARCIA*\n\n';
    msg += '👕 *Prenda:* ' + (data.garmentType || '—') + '\n';
    msg += '⚽ *Deporte:* ' + (data.sport || '—') + '\n';
    msg += '📦 *Cantidad:* ' + (data.roster.length > 0 ? data.roster.length : data.quantity) + ' uds.\n';
    if (data.sizeRangeSummary) msg += '📏 *Tallas generales:* ' + data.sizeRangeSummary + '\n';
    msg += formatRosterText(data.roster);
    msg += '👤 *Nombre/Club:* ' + data.clientName + '\n';
    if (data.clientPhone) msg += '📱 *WhatsApp:* ' + data.clientPhone + '\n';
    if (data.notes) msg += '📝 *Detalles:* ' + data.notes + '\n';
    if (panelInfo && panelInfo.orderNumber) {
      msg += '\n🧾 *Pedido registrado:* ' + panelInfo.orderNumber + '\n';
    }
    if (panelInfo && panelInfo.referenceImageUrl) {
      msg += '🖼 *Foto referencia:* ' + panelInfo.referenceImageUrl + '\n';
    } else if (referenceImageBase64 && !panelInfo) {
      msg += '\n📎 *Adjunte la foto de referencia en este chat.*\n';
    }
    msg += '\n¡Hola! Me gustaría recibir cotización. 😊';
    return msg;
  }

  function submitToPanel(data) {
    return fetch(apiBase() + '/api/landing-quote', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    }).then(function (r) {
      return r.json().then(function (body) {
        if (!r.ok) throw new Error(body.error || body.title || 'No se pudo registrar la solicitud.');
        return body;
      });
    });
  }

  function openWhatsApp(data, panelInfo) {
    var msg = buildWhatsAppMessage(data, panelInfo);
    window.open('https://wa.me/' + whatsAppPhone + '?text=' + encodeURIComponent(msg), '_blank');
  }

  function sendWhatsAppOnly() {
    var data = collectForm();
    if (!data) return;
    openWhatsApp(data, lastPanelResult);
    setStatus('Abriendo WhatsApp… Si subió foto sin registrar, adjúntela en el chat.', 'ok');
  }

  function sendPanelOnly() {
    if (isSubmitting) return;
    var data = collectForm();
    if (!data) return;
    isSubmitting = true;
    setStatus('Registrando solicitud…', '');
    submitToPanel(data)
      .then(function (res) {
        lastPanelResult = res;
        setStatus('✓ Pedido ' + res.orderNumber + ' registrado. Wilber lo verá en el panel.', 'ok');
      })
      .catch(function (err) {
        setStatus(err.message || 'Error al registrar.', 'err');
      })
      .finally(function () { isSubmitting = false; });
  }

  function sendBoth() {
    if (isSubmitting) return;
    var data = collectForm();
    if (!data) return;
    isSubmitting = true;
    setStatus('Registrando y abriendo WhatsApp…', '');
    submitToPanel(data)
      .then(function (res) {
        lastPanelResult = res;
        openWhatsApp(data, res);
        setStatus('✓ Pedido ' + res.orderNumber + ' registrado. Complete el envío en WhatsApp.', 'ok');
      })
      .catch(function (err) {
        setStatus('No se registró en panel: ' + (err.message || 'error') + '. Puede enviar solo por WhatsApp.', 'err');
      })
      .finally(function () { isSubmitting = false; });
  }

  window.sendToWhatsApp = sendWhatsAppOnly;

  function applyQuoteTexts(quote) {
    if (!quote) return;
    whatsAppPhone = (quote.whatsAppPhone || DEFAULTS.quote.whatsAppPhone).replace(/\D/g, '');
    var qty = $('quantity');
    var name = $('clientName');
    var extra = $('extraMsg');
    var note = $('quoteResponseNote');
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
    var cursor = $('cursor');
    var ring = $('cursorRing');
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

  function wireQuoteButtons() {
    var btnWa = $('btnSendWhatsApp');
    var btnPanel = $('btnSendPanel');
    var btnBoth = $('btnSendBoth');
    var btnAdd = $('btnAddRoster');
    if (btnWa) btnWa.addEventListener('click', sendWhatsAppOnly);
    if (btnPanel) btnPanel.addEventListener('click', sendPanelOnly);
    if (btnBoth) btnBoth.addEventListener('click', sendBoth);
    if (btnAdd) btnAdd.addEventListener('click', addRosterRow);
  }

  function applyConfig(data) {
    var catalog = (data && data.catalog && data.catalog.length) ? data.catalog : DEFAULTS.catalog;
    var quote = (data && data.quote) ? data.quote : DEFAULTS.quote;
    renderCatalog(catalog);
    renderGarments(quote.garments || DEFAULTS.quote.garments);
    renderSports(quote.sports || DEFAULTS.quote.sports);
    renderSizes(quote.sizes || DEFAULTS.quote.sizes);
    applyQuoteTexts(quote);
    renderRoster();
    setupPhotoUpload();
    wireQuoteButtons();
  }

  function load() {
    fetch(apiBase() + '/api/landing-config')
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
