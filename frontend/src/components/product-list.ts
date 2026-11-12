import { LitElement, html } from "lit";
import {
    getProducts,
    type Product
} from "../services/product-api";

export class ProductList extends LitElement {

    static properties = {
        products: { state: true },
        loading: { state: true },
        error: { state: true }
    };

    products: Product[] = [];
    loading = true;
    error = "";

    connectedCallback() {
        super.connectedCallback();
        this.loadProducts();
    }

    async loadProducts() {
        try {
            this.products = await getProducts();
        } catch {
            this.error = "Unable to load products.";
        } finally {
            this.loading = false;
        }
    }

    render() {
        if (this.loading) {
            return html`<p>Loading products...</p>`;
        }

        if (this.error) {
            return html`<p>${this.error}</p>`;
        }

        return html`
            <section>
                <h2>Our Products</h2>

                <div class="product-grid">
                    ${this.products.map(product => html`
                        <article class="product-card">
                            <h3>${product.productName}</h3>

                            <p>${product.productDescription}</p>

                            <strong>
                                ₱${product.categoryId}
                            </strong>

                            <p>
                                Available:
                                ${product.comments ? "In Stock" : "Out of Stock"}
                            </p>
                        </article>
                    `)}
                </div>
            </section>
        `;
    }
}

customElements.define(
    "product-list",
    ProductList
);