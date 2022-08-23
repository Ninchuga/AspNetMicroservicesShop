import { OrderItem } from "./OrderItem";

export interface PlaceOrder {
    userId: string;
    userName: string;
    totalPrice: number;
    firstName: string;
    lastName: string;
    email:string;
    street: string;
    country: string;
    city: string;
    cardName: string;
    cardNumber: string;
    cardExpiration: string;
    cvv: number;
    orderItems: Array<OrderItem>;
}