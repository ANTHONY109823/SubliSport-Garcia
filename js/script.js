// Inicializar Animaciones
AOS.init();

// Elementos del Simulador
const colorPolo = document.getElementById('color-polo');
const colorShort = document.getElementById('color-short');
const colorMedias = document.getElementById('color-medias');

const svgPolo = document.getElementById('svg-polo');
const svgShort = document.getElementById('svg-short');
const svgMedias = document.getElementById('svg-medias');
const svgMediasR = document.getElementById('svg-medias-r');

const btnPedir = document.getElementById('btn-pedir');

// Cambiar colores en tiempo real
colorPolo.addEventListener('input', (e) => {
    svgPolo.style.fill = e.target.value;
});

colorShort.addEventListener('input', (e) => {
    svgShort.style.fill = e.target.value;
});

colorMedias.addEventListener('input', (e) => {
    svgMedias.style.fill = e.target.value;
    svgMediasR.style.fill = e.target.value;
});

// Botón de WhatsApp con los colores elegidos
btnPedir.addEventListener('click', () => {
    const msg = `Hola Sublisport Garcia, he diseñado un uniforme:
- Polo: ${colorPolo.value}
- Short: ${colorShort.value}
- Medias: ${colorMedias.value}
¿Podrían darme una cotización?`;
    
    const url = `https://wa.me/519XXXXXXXX?text=${encodeURIComponent(msg)}`;
    window.open(url, '_blank');
});