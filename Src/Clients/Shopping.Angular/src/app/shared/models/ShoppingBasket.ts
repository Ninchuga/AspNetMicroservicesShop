import { ShoppingBasketItem } from "./ShoppingBasketItem";

export interface ShoppingBasket {
    items: Array<ShoppingBasketItem>,
    totalPrice: number
}