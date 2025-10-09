// This script is primarily for dynamically interacting with filter elements if needed,
// but the current filtering logic is handled by form submission on checkbox change.
// The expandable sidebar functionality is included here.

document.addEventListener('DOMContentLoaded', function () {
    // Toggle expand/collapse functionality for sidebar sections
    document.querySelectorAll('.expandable-header').forEach(header => {
        header.addEventListener('click', function () {
            this.parentElement.classList.toggle('expanded');
        });
    });

    // Note: The original JS provided had functionality for local storage favorites
    // and handling "select all" checkboxes. This logic has been removed as
    // the current implementation relies on server-side filtering and database-driven
    // state for things like favorites. If you want to add purely client-side
    // features back in, this is the place to do it.
});