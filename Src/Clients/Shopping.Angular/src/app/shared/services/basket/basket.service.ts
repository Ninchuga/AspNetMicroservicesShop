import { HttpClient, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { catchError, retry, Observable } from 'rxjs';
import { ShoppingBasketItem } from '../../models/ShoppingBasketItem';
import { ShoppingErrorHandler } from 'src/app/errorHandler';
import { CatalogItem } from '../../models/CatalogItem';
import { ShoppingBasket } from '../../models/ShoppingBasket';
import { APP_CONFIG, Settings } from 'src/app/settings';

@Injectable({
  providedIn: 'root'
})
export class BasketService {
  globalSettings: Settings;

  constructor(private http: HttpClient,
              private shoppingErrorHandler: ShoppingErrorHandler,
              @Inject(APP_CONFIG)settings: Settings) 
              { 
                this.globalSettings = settings;
              }

  getUserBasket(userId: string): Observable<HttpResponse<ShoppingBasket>> {
    console.log('get basket...');
    let url = `${this.globalSettings.apiGatewayBaseUrl}/Basket/api/${userId}`;
    let headers = new HttpHeaders()
                        .set('Accept', 'application/json');

    return this.http
        .get<ShoppingBasket>(url, { observe: 'response', headers: headers })
        .pipe(
          retry(3), // retry a failed request up to 3 times
          catchError(this.shoppingErrorHandler.handleError) // then handle the error
        );
  }

  addItemToBasket(item: CatalogItem): Observable<HttpResponse<ShoppingBasketItem>> {
    let url = `${this.globalSettings.apiGatewayBaseUrl}/Basket/api/AddBasketItem`;

    let headers = new HttpHeaders()
                        .set('Content-Type', 'application/json');

    const shoppingBasketItem: ShoppingBasketItem = {
      productId: item.id,
      productName: item.name,
      quantity: item.quantity,
      price: item.price,
      discount: 0,
      priceWithDiscount: 0
    }

    return this.http
      .post<ShoppingBasketItem>(url, shoppingBasketItem, { observe: 'response', headers: headers })
      .pipe(
        catchError(this.shoppingErrorHandler.handleError)
      );
  }

  deleteBasketItem(productId: string): Observable<HttpResponse<any>> {
    let url = `${this.globalSettings.apiGatewayBaseUrl}/Basket/api/DeleteBasketItem/${productId}`;

    let headers = new HttpHeaders()
                        .set('Content-Type', 'application/json');

    return this.http
      .delete(url, { headers, observe: 'response' })
      .pipe(
        catchError(this.shoppingErrorHandler.handleError)
      );
  }
}
