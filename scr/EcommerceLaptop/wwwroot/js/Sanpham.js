
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.expandable-header').forEach(header => {
        header.addEventListener('click', function () {
            this.parentElement.classList.toggle('expanded');
        });
    });
});