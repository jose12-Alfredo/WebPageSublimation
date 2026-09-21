function syncVariants(row) {
    const product = row?.querySelector("[data-product-select]");
    const variant = row?.querySelector("[data-variant-select]");
    if (!product || !variant) return;
    const productId = product.value;
    let selectedStillAvailable = variant.value === "";
    for (const option of variant.options) {
        if (!option.dataset.productId) continue;
        const visible = productId !== "" && option.dataset.productId === productId;
        option.hidden = !visible;
        option.disabled = !visible;
        if (visible && option.value === variant.value) selectedStillAvailable = true;
    }
    if (!selectedStillAvailable) variant.value = "";
}

document.addEventListener("change", event => {
    if (event.target.matches("[data-product-select]")) syncVariants(event.target.closest("[data-line]"));
});

document.addEventListener("click", event => {
    const add = event.target.closest("[data-add-line]");
    if (add) {
        const template = document.getElementById("proforma-line-template");
        const lines = document.querySelector("[data-proforma-form] [data-lines]");
        if (template && lines) {
            lines.append(template.content.cloneNode(true));
            const row = lines.lastElementChild;
            syncVariants(row);
            row?.querySelector("select")?.focus();
        }
        return;
    }

    const remove = event.target.closest("[data-remove-line]");
    if (!remove) return;
    const lines = remove.closest("[data-lines]");
    const row = remove.closest("[data-line]");
    if (!lines || !row) return;
    if (lines.querySelectorAll("[data-line]").length > 1) row.remove();
    else {
        row.querySelectorAll("select,input").forEach(input => input.value = "");
        syncVariants(row);
        row.querySelector("select")?.focus();
    }
});

document.querySelectorAll("[data-line]").forEach(syncVariants);
