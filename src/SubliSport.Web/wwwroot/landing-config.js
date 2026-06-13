/**
 * Catálogo y cotización automática (lista jugadores, foto, panel + WhatsApp).
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
      responseNote: 'Wilber revisará su pedido y le enviará la proforma con precios',
      quantityPlaceholder: 'Cantidad de prendas',
      namePlaceholder: 'Tu nombre o club *',
      extraPlaceholder: 'Colores, diseño, fecha de entrega, escudo...',
      garments: [
        { label: 'Conjunto Completo', value: 'Conjunto completo (camiseta + short + medias)', iconClass: 'fas fa-tshirt' },
        { label: 'Solo Camiseta', value: 'Solo camiseta', iconClass: 'fas fa-circle-dot' },
        { label: 'Solo Short', value: 'Short deportivo', iconClass: 'fas fa-person-running' },
        { label: 'Ambos tipos', value: 'Mixta', iconClass: 'fas fa-layer-group', isMixed: true }
      ],
      mixedTypes: [
        'Conjunto completo',
        'Polo / Camiseta sola',
        'Short / Pantaloneta',
        'Medias',
        'Otro'
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
      ],
      fabrics: [
        { key: 'dry_fit', label: 'DRY FIT' },
        { key: 'win_fresch', label: 'WIN FRESCH' },
        { key: 'poly_exagonal', label: 'POLY EXAGONAL' },
        { key: 'puma', label: 'PUMA' },
        { key: 'gota', label: 'GOTA' },
        { key: 'sig_sag', label: 'SIG SAG' },
        { key: 'marathon', label: 'MARATON' },
        { key: 'micro_nike', label: 'MICRO NIKE' },
        { key: 'labrado_brillo', label: 'LABRADO CON BRILLO' }
      ]
    }
  };

  var selectedPrenda = '';
  var selectedSport = '';
  var selectedClientType = 'direct';
  var isMixedMode = false;
  var whatsAppPhone = DEFAULTS.quote.whatsAppPhone;
  var rosterRows = [{ name: '', size: '', number: '' }];
  var mixedLines = [{ itemType: 'Conjunto completo', quantity: 1, other: '' }];
  var mixedTypes = DEFAULTS.quote.mixedTypes;
  var referenceImagesBase64 = [null, null, null];
  var MAX_REFERENCE_PHOTOS = 3;
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

  function updateClientTypeUi() {
    var panel = $('serviceInfoPanel');
    if (panel) panel.classList.toggle('panel-hidden', selectedClientType !== 'service');
    var wrap = $('clientTypeOpts');
    if (!wrap) return;
    wrap.querySelectorAll('.client-type-card').forEach(function (el) {
      var type = el.getAttribute('data-type');
      el.classList.toggle('active', type === selectedClientType);
    });
  }

  function renderClientTypes() {
    var wrap = $('clientTypeOpts');
    if (!wrap) return;
    wrap.innerHTML =
      '<button type="button" class="client-type-card client-type-retail" data-type="direct">' +
        '<span class="client-type-icon"><i class="fas fa-users"></i></span>' +
        '<span class="client-type-tag">Retail</span>' +
        '<strong class="client-type-title">Cliente directo</strong>' +
        '<p class="client-type-desc">Persona natural o club — cotización por prenda completa.</p>' +
        '<ul class="client-type-features"><li>Precio por conjunto</li><li>Ideal equipos</li></ul>' +
        '<span class="client-type-check">✓</span>' +
      '</button>' +
      '<button type="button" class="client-type-card client-type-service" data-type="service">' +
        '<span class="client-type-icon"><i class="fas fa-industry"></i></span>' +
        '<span class="client-type-tag">B2B</span>' +
        '<strong class="client-type-title">Por servicio</strong>' +
        '<p class="client-type-desc">Taller o empresa — diseño, impresión, planchado y confección opcional.</p>' +
        '<ul class="client-type-features"><li>Precio por metraje</li><li>Servicio profesional</li></ul>' +
        '<span class="client-type-check">✓</span>' +
      '</button>';
    wrap.querySelectorAll('.client-type-card').forEach(function (el) {
      el.addEventListener('click', function () {
        selectedClientType = el.getAttribute('data-type') || 'direct';
        updateClientTypeUi();
      });
    });
    updateClientTypeUi();
    wireHover(wrap.querySelectorAll('.client-type-card'));
  }

  function setMixedMode(mixed) {
    isMixedMode = mixed;
    var panel = $('mixedOrderPanel');
    var qtyWrap = $('singleQuantityWrap');
    if (panel) panel.classList.toggle('visible', mixed);
    if (qtyWrap) qtyWrap.classList.toggle('panel-hidden', mixed);
  }

  function isMixedGarment(g) {
    return !!(g && (g.isMixed || (g.value && String(g.value).toLowerCase() === 'mixta')));
  }

  function ensureGarmentOptions(garments) {
    var list = garments && garments.length
      ? garments.map(function (g) {
          return {
            label: g.label,
            value: g.value,
            iconClass: g.iconClass || 'fas fa-tshirt',
            isMixed: isMixedGarment(g)
          };
        })
      : DEFAULTS.quote.garments.slice();
    if (!list.some(isMixedGarment)) {
      list.push({
        label: 'Ambos tipos',
        value: 'Mixta',
        iconClass: 'fas fa-layer-group',
        isMixed: true
      });
    }
    return list;
  }

  function renderFabrics(fabrics) {
    var select = $('fabricType');
    if (!select) return;
    var list = fabrics && fabrics.length ? fabrics : DEFAULTS.quote.fabrics;
    select.innerHTML = list.map(function (f, i) {
      var key = f.key || f.Key || '';
      var label = f.label || f.Label || key;
      return '<option value="' + esc(key) + '" style="background:#111"' + (i === 0 ? ' selected' : '') + '>' + esc(label) + '</option>';
    }).join('');
  }

  function renderGarments(garments) {
    var wrap = $('garmentOpts');
    if (!wrap) return;
    var options = ensureGarmentOptions(garments);
    wrap.innerHTML = options.map(function (g, i) {
      return '<div class="prenda-opt' + (i === 0 ? ' active' : '') + '" data-value="' + esc(g.value) + '"' +
        (g.isMixed ? ' data-mixed="1"' : '') + '>' +
        '<i class="' + esc(g.iconClass || 'fas fa-tshirt') + '"></i><span>' + esc(g.label) + '</span></div>';
    }).join('');
    selectedPrenda = options[0].value;
    setMixedMode(!!options[0].isMixed);
    wrap.querySelectorAll('.prenda-opt').forEach(function (el) {
      el.addEventListener('click', function () {
        wrap.querySelectorAll('.prenda-opt').forEach(function (d) { d.classList.remove('active'); });
        el.classList.add('active');
        selectedPrenda = el.getAttribute('data-value') || '';
        setMixedMode(el.getAttribute('data-mixed') === '1');
      });
    });
    wireHover(wrap.querySelectorAll('.prenda-opt'));
  }

  function renderMixedLines() {
    var body = $('mixedLinesBody');
    if (!body) return;
    body.innerHTML = mixedLines.map(function (row, i) {
      var opts = mixedTypes.map(function (t) {
        return '<option value="' + esc(t) + '"' + (row.itemType === t ? ' selected' : '') + ' style="background:#111">' + esc(t) + '</option>';
      }).join('');
      var otherField = row.itemType === 'Otro'
        ? '<input type="text" data-mixed-other value="' + esc(row.other) + '" placeholder="Describa la prenda" style="grid-column:1/-1;margin-top:6px;background:rgba(255,255,255,0.04);border:1px solid var(--border);color:var(--white);padding:10px;border-radius:8px;">'
        : '';
      return '<div class="mixed-row" data-midx="' + i + '">' +
        '<select data-mixed-type>' + opts + '</select>' +
        '<input type="number" data-mixed-qty min="1" value="' + (row.quantity || 1) + '">' +
        (mixedLines.length > 1 ? '<button type="button" class="btn-roster-del" data-mixed-remove="' + i + '">✕</button>' : '<span></span>') +
        otherField + '</div>';
    }).join('');

    body.querySelectorAll('[data-mixed-type]').forEach(function (sel) {
      sel.addEventListener('change', function () {
        var row = sel.closest('.mixed-row');
        var idx = parseInt(row.getAttribute('data-midx'), 10);
        if (!isNaN(idx) && mixedLines[idx]) {
          mixedLines[idx].itemType = sel.value;
          renderMixedLines();
        }
      });
    });
    body.querySelectorAll('[data-mixed-qty]').forEach(function (inp) {
      inp.addEventListener('input', function () {
        var row = inp.closest('.mixed-row');
        var idx = parseInt(row.getAttribute('data-midx'), 10);
        if (!isNaN(idx) && mixedLines[idx]) mixedLines[idx].quantity = parseInt(inp.value, 10) || 1;
      });
    });
    body.querySelectorAll('[data-mixed-other]').forEach(function (inp) {
      inp.addEventListener('input', function () {
        var row = inp.closest('.mixed-row');
        var idx = parseInt(row.getAttribute('data-midx'), 10);
        if (!isNaN(idx) && mixedLines[idx]) mixedLines[idx].other = inp.value;
      });
    });
    body.querySelectorAll('[data-mixed-remove]').forEach(function (btn) {
      btn.addEventListener('click', function () {
        var idx = parseInt(btn.getAttribute('data-mixed-remove'), 10);
        if (mixedLines.length > 1) {
          mixedLines.splice(idx, 1);
          renderMixedLines();
        }
      });
    });
  }

  function addMixedLine() {
    mixedLines.push({ itemType: mixedTypes[0], quantity: 1, other: '' });
    renderMixedLines();
  }

  function getMixedLinesPayload() {
    return mixedLines
      .filter(function (l) { return l.quantity > 0; })
      .map(function (l) {
        return {
          itemType: l.itemType,
          quantity: l.quantity,
          otherDescription: l.itemType === 'Otro' ? (l.other || '').trim() : ''
        };
      });
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

  function getReferenceImages() {
    return referenceImagesBase64.filter(function (img) { return !!img; });
  }

  function photoStatusMessage(count) {
    if (!count) return '';
    return count + ' foto' + (count === 1 ? '' : 's') + ' lista' + (count === 1 ? '' : 's') + ' para enviar.';
  }

  function setupPhotoUpload() {
    var slots = document.querySelectorAll('.photo-slot');
    if (!slots.length) return;

    slots.forEach(function (slot) {
      var input = slot.querySelector('.photo-slot-input');
      var preview = slot.querySelector('.photo-slot-preview');
      var removeBtn = slot.querySelector('.photo-slot-remove');
      var idx = parseInt(slot.getAttribute('data-slot'), 10);
      if (!input || idx < 0 || idx >= MAX_REFERENCE_PHOTOS) return;

      input.addEventListener('change', function () {
        var file = input.files && input.files[0];
        if (!file) return;
        if (file.size > 4 * 1024 * 1024) {
          setStatus('La imagen supera 4 MB. Elija otra.', 'err');
          input.value = '';
          return;
        }
        compressImage(file, function (dataUrl) {
          referenceImagesBase64[idx] = dataUrl;
          if (preview) preview.src = dataUrl;
          slot.classList.add('has-photo');
          setStatus(photoStatusMessage(getReferenceImages().length), 'ok');
        });
      });

      if (removeBtn) {
        removeBtn.addEventListener('click', function (e) {
          e.stopPropagation();
          e.preventDefault();
          referenceImagesBase64[idx] = null;
          input.value = '';
          if (preview) preview.removeAttribute('src');
          slot.classList.remove('has-photo');
          var count = getReferenceImages().length;
          setStatus(photoStatusMessage(count), count ? 'ok' : '');
        });
      }
    });
  }

  function normalizeHeader(h) {
    return String(h || '').toLowerCase()
      .normalize('NFD').replace(/[\u0300-\u036f]/g, '')
      .trim();
  }

  function parseRosterRows(rows) {
    if (!rows || !rows.length) return [];
    var header = rows[0].map(normalizeHeader);
    var hasHeader = header.some(function (h) {
      return h.indexOf('nombre') >= 0 || h.indexOf('talla') >= 0 || h === 'n' || h.indexOf('numero') >= 0;
    });
    var dataRows = hasHeader ? rows.slice(1) : rows;
    var nameIdx = 0;
    var sizeIdx = 1;
    var numIdx = 2;
    if (hasHeader) {
      nameIdx = header.findIndex(function (h) { return h.indexOf('nombre') >= 0; });
      sizeIdx = header.findIndex(function (h) { return h.indexOf('talla') >= 0; });
      numIdx = header.findIndex(function (h) { return h.indexOf('numero') >= 0 || h === 'n' || h.indexOf('nro') >= 0; });
      if (nameIdx < 0) nameIdx = 0;
      if (sizeIdx < 0) sizeIdx = 1;
      if (numIdx < 0) numIdx = 2;
    }
    return dataRows.map(function (row) {
      return {
        name: String(row[nameIdx] || '').trim(),
        size: String(row[sizeIdx] || '').trim(),
        number: String(row[numIdx] || '').trim()
      };
    }).filter(function (r) { return r.name || r.size || r.number; });
  }

  function downloadRosterTemplate() {
    var csv = 'Nombre,Talla,Numero\nEjemplo Juan Perez,M,10\nEjemplo Maria Lopez,S,7\n';
    var blob = new Blob(['\ufeff' + csv], { type: 'text/csv;charset=utf-8' });
    var url = URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.href = url;
    a.download = 'plantilla-lista-sublisport.csv';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    setStatus('Plantilla descargada. Ábrala en Excel, complete y súbala aquí.', 'ok');
  }

  function setupExcelUpload() {
    var input = $('rosterExcel');
    var zone = $('excelZone');
    var label = $('excelFileName');
    if (!input) return;

    input.addEventListener('change', function () {
      var file = input.files && input.files[0];
      if (!file) return;
      if (typeof XLSX === 'undefined') {
        setStatus('No se pudo cargar el lector de Excel. Use la plantilla CSV.', 'err');
        return;
      }
      var reader = new FileReader();
      reader.onload = function (e) {
        try {
          var data = new Uint8Array(e.target.result);
          var workbook = XLSX.read(data, { type: 'array' });
          var sheet = workbook.Sheets[workbook.SheetNames[0]];
          var rows = XLSX.utils.sheet_to_json(sheet, { header: 1, defval: '' });
          var parsed = parseRosterRows(rows);
          if (!parsed.length) {
            setStatus('No se encontraron filas válidas. Use la plantilla.', 'err');
            return;
          }
          rosterRows = parsed.length ? parsed : [{ name: '', size: '', number: '' }];
          renderRoster();
          if (zone) zone.classList.add('has-file');
          if (label) label.textContent = file.name + ' · ' + parsed.length + ' filas cargadas';
          setStatus(parsed.length + ' jugadores importados desde Excel.', 'ok');
        } catch (err) {
          setStatus('No se pudo leer el archivo. Verifique el formato.', 'err');
        }
      };
      reader.readAsArrayBuffer(file);
    });
  }

  function addBusinessDays(fromDate, businessDays) {
    var d = new Date(fromDate.getTime());
    var added = 0;
    while (added < businessDays) {
      d.setDate(d.getDate() + 1);
      if (d.getDay() !== 0 && d.getDay() !== 6) added++;
    }
    return d;
  }

  function toIsoDate(d) {
    var y = d.getFullYear();
    var m = String(d.getMonth() + 1).padStart(2, '0');
    var day = String(d.getDate()).padStart(2, '0');
    return y + '-' + m + '-' + day;
  }

  function formatDateEs(iso) {
    if (!iso) return '';
    var p = iso.split('-');
    if (p.length !== 3) return iso;
    return p[2] + '/' + p[1] + '/' + p[0];
  }

  function setupDeliveryDate() {
    var input = $('desiredDelivery');
    var btn = $('btnSuggestDelivery');
    var hint = $('deliveryHint');
    if (!input) return;

    var today = new Date();
    today.setHours(0, 0, 0, 0);
    input.min = toIsoDate(today);

    var suggested = addBusinessDays(today, 7);
    var suggestedIso = toIsoDate(suggested);

    if (hint) {
      hint.innerHTML = 'Recomendamos entrega a partir del <strong style="color:var(--gold);">' +
        formatDateEs(suggestedIso) + '</strong> (7 días hábiles). Si necesita más tiempo, elija otra fecha en el calendario.';
    }

    if (btn) {
      btn.addEventListener('click', function () {
        input.value = suggestedIso;
        setStatus('Fecha sugerida aplicada: ' + formatDateEs(suggestedIso), 'ok');
      });
    }
  }

  function collectForm() {
    var qty = $('quantity');
    var size = $('sizeRange');
    var name = $('clientName');
    var phone = $('clientPhone');
    var extra = $('extraMsg');
    var fabric = $('fabricType');
    var delivery = $('desiredDelivery');
    var chkEscudo = $('chkEmbroideryEscudo');
    var chkMarca = $('chkEmbroideryMarca');
    var chkShort = $('chkEmbroideryShort');
    var roster = getRosterFromDom();
    var mixed = getMixedLinesPayload();

    if (!name || !name.value.trim()) {
      setStatus('Indique su nombre o club.', 'err');
      name && name.focus();
      return null;
    }

    if (!phone || !phone.value.trim()) {
      setStatus('Indique su WhatsApp para que el asesor le contacte.', 'err');
      phone && phone.focus();
      return null;
    }

    if (isMixedMode && !mixed.length) {
      setStatus('Agregue al menos una línea al pedido mixto.', 'err');
      return null;
    }

    return {
      clientName: name.value.trim(),
      clientPhone: phone.value.trim(),
      garmentType: isMixedMode ? 'Mixta' : selectedPrenda,
      sport: selectedSport,
      quantity: isMixedMode
        ? mixed.reduce(function (s, l) { return s + l.quantity; }, 0)
        : (qty && qty.value ? parseInt(qty.value, 10) || 1 : Math.max(1, roster.length)),
      sizeRangeSummary: size && size.value ? size.value : '',
      desiredDeliveryDeadline: delivery && delivery.value
        ? formatDateEs(delivery.value.trim())
        : '',
      notes: extra && extra.value ? extra.value.trim() : '',
      roster: roster,
      mixedLines: isMixedMode ? mixed : [],
      referenceImagesBase64: getReferenceImages(),
      referenceImageBase64: getReferenceImages()[0] || null,
      fabricKey: fabric && fabric.value ? fabric.value : 'dry_fit',
      embroideryEscudo: !!(chkEscudo && chkEscudo.checked),
      embroideryMarca: !!(chkMarca && chkMarca.checked),
      embroideryShort: !!(chkShort && chkShort.checked),
      pricingTier: selectedClientType === 'service' ? 1 : 0
    };
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

  function openBusinessWhatsApp(text) {
    window.open('https://wa.me/' + whatsAppPhone + '?text=' + encodeURIComponent(text), '_blank');
  }

  function sendQuote() {
    if (isSubmitting) return;
    var data = collectForm();
    if (!data) return;

    isSubmitting = true;
    setStatus('Registrando solicitud…', '');

    submitToPanel(data)
      .then(function (res) {
        var waText = res.clientRequestText || '';
        if (res.orderNumber) {
          waText += '\n\n🧾 *Referencia:* ' + res.orderNumber;
        }
        var refUrls = res.referenceImageUrls && res.referenceImageUrls.length
          ? res.referenceImageUrls
          : (res.referenceImageUrl ? [res.referenceImageUrl] : []);
        if (refUrls.length) {
          refUrls.forEach(function (url, i) {
            var label = refUrls.length > 1 ? 'Foto referencia ' + (i + 1) : 'Foto referencia';
            waText += '\n🖼 *' + label + ':* ' + url;
          });
        } else if (getReferenceImages().length) {
          waText += '\n\n📎 *Adjunte las fotos de referencia en este chat.*';
        }

        openBusinessWhatsApp(waText);
        setStatus(
          '✓ Solicitud ' + (res.orderNumber || '') + ' registrada en el panel de Wilber. Complete el envío por WhatsApp; el asesor le enviará la proforma.',
          'ok'
        );
      })
      .catch(function (err) {
        setStatus(err.message || 'Error al enviar la cotización.', 'err');
      })
      .finally(function () { isSubmitting = false; });
  }

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
    var btn = $('btnSendQuote');
    var btnAdd = $('btnAddRoster');
    var btnMixed = $('btnAddMixedLine');
    var btnTemplate = $('btnDownloadTemplate');
    if (btn) btn.addEventListener('click', sendQuote);
    if (btnAdd) btnAdd.addEventListener('click', addRosterRow);
    if (btnMixed) btnMixed.addEventListener('click', addMixedLine);
    if (btnTemplate) btnTemplate.addEventListener('click', downloadRosterTemplate);
  }

  function applyConfig(data) {
    var catalog = (data && data.catalog && data.catalog.length) ? data.catalog : DEFAULTS.catalog;
    var quote = (data && data.quote) ? data.quote : DEFAULTS.quote;
    renderCatalog(catalog);
    renderClientTypes();
    renderGarments(quote.garments || DEFAULTS.quote.garments);
    renderSports(quote.sports || DEFAULTS.quote.sports);
    renderSizes(quote.sizes || DEFAULTS.quote.sizes);
    renderFabrics(quote.fabrics || DEFAULTS.quote.fabrics);
    applyQuoteTexts(quote);
    mixedTypes = quote.mixedTypes || DEFAULTS.quote.mixedTypes;
    renderRoster();
    renderMixedLines();
    setupPhotoUpload();
    setupExcelUpload();
    setupDeliveryDate();
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
