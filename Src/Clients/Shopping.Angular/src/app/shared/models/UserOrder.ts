import { OrderItem } from "./OrderItem";

export interface UserOrder {
    orderId: string;
    userId: string;
    userName: string;
    totalPrice: number;
    orderStatus: string;
    orderPaid: string;
    orderDate: string;
    orderCancelationDate: string;
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