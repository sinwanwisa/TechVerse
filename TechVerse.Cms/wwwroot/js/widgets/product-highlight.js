function scrollProducts(direction) {
    const wrapper = document.getElementById('productScrollWrapper');
    if (!wrapper) return;

    const card = wrapper.querySelector('.product-card');
    const cardWidth = card ? card.offsetWidth : 260;
    const gap = 32; // 2rem

    wrapper.scrollBy({
        left: direction * (cardWidth + gap),
        behavior: 'smooth'
    });
}