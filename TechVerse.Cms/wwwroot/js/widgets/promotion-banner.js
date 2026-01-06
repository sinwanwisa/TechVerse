var currentIndex = 0;
var totalSlides = 0;

function updateBanner() {
    var track = document.getElementById('bannerTrack');
    if (!track) return;

    track.style.transform = 'translateX(-' + (100 * currentIndex) + '%)';
}

function slideBanner(direction) {
    if (totalSlides === 0) return;

    currentIndex += direction;

    if (currentIndex < 0) currentIndex = totalSlides - 1;
    if (currentIndex >= totalSlides) currentIndex = 0;

    updateBanner();
}

document.addEventListener('DOMContentLoaded', function () {
    var track = document.getElementById('bannerTrack');
    if (!track) return;

    totalSlides = parseInt(track.dataset.total || '0', 10);
    updateBanner();
});
