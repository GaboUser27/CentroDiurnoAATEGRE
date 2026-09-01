// Carrusel de Avisos — AATEGRE
// Componente propio (sin depender del carousel básico de Bootstrap)

document.addEventListener('DOMContentLoaded', function () {

    const wrap = document.querySelector('[data-carrusel-avisos]');
    if (!wrap) return;

    const track = wrap.querySelector('.carrusel-track');
    const slides = Array.from(wrap.querySelectorAll('.carrusel-slide'));
    const dotsContenedor = wrap.querySelector('.carrusel-dots');
    const btnPrev = wrap.querySelector('.carrusel-flecha--prev');
    const btnNext = wrap.querySelector('.carrusel-flecha--next');
    const numeroActual = wrap.querySelector('.carrusel-num-actual');
    const barraProgreso = wrap.querySelector('.carrusel-progreso-barra');

    if (slides.length === 0) return;

    // Si solo hay un aviso, no tiene sentido mostrar controles de navegación
    const soloUno = slides.length === 1;

    let indice = 0;
    const INTERVALO = 6000;
    let temporizador = null;

    // Construir dots dinámicamente
    slides.forEach((_, i) => {
        const dot = document.createElement('button');
        dot.type = 'button';
        dot.className = 'carrusel-dot' + (i === 0 ? ' activo' : '');
        dot.setAttribute('aria-label', 'Ir al aviso ' + (i + 1));
        dot.addEventListener('click', () => irA(i));
        dotsContenedor.appendChild(dot);
    });

    const dots = Array.from(dotsContenedor.querySelectorAll('.carrusel-dot'));

    function render() {
        track.style.transform = `translateX(-${indice * 100}%)`;

        slides.forEach((slide, i) => slide.classList.toggle('activo', i === indice));
        dots.forEach((dot, i) => dot.classList.toggle('activo', i === indice));

        if (numeroActual) numeroActual.textContent = String(indice + 1).padStart(2, '0');
    }

    function irA(nuevoIndice) {
        indice = (nuevoIndice + slides.length) % slides.length;
        render();
        reiniciarAutoplay();
    }

    function siguiente() { irA(indice + 1); }
    function anterior() { irA(indice - 1); }

    function reiniciarProgreso() {
        if (!barraProgreso) return;
        barraProgreso.classList.remove('animando');
        // Forzar reflow para poder reiniciar la animación de ancho
        void barraProgreso.offsetWidth;
        if (!soloUno) barraProgreso.classList.add('animando');
    }

    function reiniciarAutoplay() {
        clearInterval(temporizador);
        reiniciarProgreso();
        if (soloUno) return;
        temporizador = setInterval(siguiente, INTERVALO);
    }

    if (btnNext) btnNext.addEventListener('click', siguiente);
    if (btnPrev) btnPrev.addEventListener('click', anterior);

    if (soloUno) {
        if (btnNext) btnNext.style.display = 'none';
        if (btnPrev) btnPrev.style.display = 'none';
        if (dotsContenedor) dotsContenedor.style.display = 'none';
    }

    // Pausar autoplay al pasar el mouse o al hacer foco (accesibilidad)
    wrap.addEventListener('mouseenter', () => {
        clearInterval(temporizador);
        if (barraProgreso) barraProgreso.classList.remove('animando');
    });
    wrap.addEventListener('mouseleave', reiniciarAutoplay);
    wrap.addEventListener('focusin', () => clearInterval(temporizador));
    wrap.addEventListener('focusout', reiniciarAutoplay);

    // Navegación con teclado cuando el carrusel tiene el foco
    wrap.setAttribute('tabindex', '0');
    wrap.addEventListener('keydown', (e) => {
        if (e.key === 'ArrowRight') siguiente();
        if (e.key === 'ArrowLeft') anterior();
    });

    // Soporte de swipe táctil
    let xInicial = null;
    track.addEventListener('touchstart', (e) => {
        xInicial = e.touches[0].clientX;
        clearInterval(temporizador);
    }, { passive: true });

    track.addEventListener('touchend', (e) => {
        if (xInicial === null) return;
        const xFinal = e.changedTouches[0].clientX;
        const diferencia = xInicial - xFinal;
        if (Math.abs(diferencia) > 40) {
            diferencia > 0 ? siguiente() : anterior();
        } else {
            reiniciarAutoplay();
        }
        xInicial = null;
    });

    render();
    reiniciarAutoplay();
});