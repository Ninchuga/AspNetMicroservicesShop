import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { ShoppingErrorHandler } from 'src/app/errorHandler';
import { ShoppingBasket } from '../../models/ShoppingBasket';
import { catchError, mergeMap, retry } from 'rxjs/operators';
import { UserOrder } from '../../models/UserOrder';
import { ShoppingBasketItem } from '../../models/ShoppingBasketItem';
import { OrderItem } from '../../models/OrderItem';
import { CheckoutData } from '../../models/CheckoutData';
import { PlaceOrder } from '../../models/PlaceOrder';
import { APP_CONFIG, Settings } from 'src/app/settings';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  globalSettings: Settings;

  constructor(private http: HttpClient,
              private shoppingErrorHandler: ShoppingErrorHandler,
              @Inject(APP_CONFIG)settings: Settings) 
              { 
                this.globalSettings = settings;
              }

  getUserOrders(userId: string) {
    const url = `${this.globalSettings.apiGatewayBaseUrl}/Order/api/GetOrders/${userId}`;
    let headers = new HttpHeaders()
                        .set('Accept', 'application/json');

    return this.http.get<UserOrder[]>(url, { observe: 'response', headers })
      .pipe(
        retry(3),
        catchError(this.shoppingErrorHandler.handleError)
      );
  }

  placeOrder(checkoutData: CheckoutData) {
    console.log('placing order...');

    return this.getUserShoppingBasket(checkoutData.userId)
    .pipe(
      retry(3),
      catchError(this.shoppingErrorHandler.handleError),
      mergeMap(basket => 
            this.placeUserOrder(checkoutData, basket)
            .pipe(
              catchError(this.shoppingErrorHandler.handleError)
            )
        )
    );
  }

  private getUserShoppingBasket(userId: string){
    const basketApiUrl = `${this.globalSettings.apiGatewayBaseUrl}/Basket/api/${userId}`;
    let basketApiHeaders = new HttpHeaders()
                        .set('Accept', 'application/json');

    return this.http.get<ShoppingBasket>(basketApiUrl, { headers: basketApiHeaders });
  }

  private placeUserOrder(checkoutData: CheckoutData, basket: ShoppingBasket){
    const orderApiUrl = `${this.globalSettings.apiGatewayBaseUrl}/Order/api/PlaceOrder`;
    let orderApiHeaders = new HttpHeaders()
                        .set('Content-Type', 'application/json');

    return this.http.put<UserOrder>(orderApiUrl, this.buildPlaceOrder(checkoutData, basket.totalPrice, this.mapToOrderItems(basket.items)), 
                        { observe: 'response', headers: orderApiHeaders });
  }

  private buildPlaceOrder(data: CheckoutData, totalPrice: number, orderItems: OrderItem[]): PlaceOrder {
    return {
      userId: data.userId,
      userName: data.userName,
      firstName: data.firstName,
      lastName: data.lastName,
      email: data.email,
      street: data.street,
      city: data.city,
      country: data.country,
      cardName: data.cardName,
      cardNumber: data.cardNumber,
      cardExpiration: data.cardExpiration,
      cvv: data.cvv,
      totalPrice: totalPrice,
      orderItems: orderItems
    } as PlaceOrder;
  }

  private mapToOrderItems(basketItems: ShoppingBasketItem[]): OrderItem[] {
    return basketItems.map(item => 
      (
        {
          productId: item.productId,
          productName: item.productName,
          price: item.price,
          discount: item.discount,
          quantity: item.quantity
        }
      )) as OrderItem[];
  }
}
