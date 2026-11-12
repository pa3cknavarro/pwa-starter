const API_URL = "http://localhost:5000/api/v1";

export interface Product {
    productId: string;
    productName: string;
    productDescription: string;
    categoryId: string;
    productImageUrl: string;
    inStock: boolean;
    comments: string;
    createdBy: string;
    createdOn: Date;
}

export async function getProducts(): Promise<Product[]> {
    const response = await fetch(`${API_URL}/products`);

    if (!response.ok) {
        throw new Error("Unable to load products");
    }

    return await response.json();
}