import { HttpClient, HttpHeaders, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { catchError, retry, Observable } from 'rxjs';
import { Constants } from '../../constants';
import { ShoppingBasketItem } from '../../models/ShoppingBasketItem';
import { ShoppingErrorHandler } from 'src/app/errorHandler';
import { CatalogItem } from '../../models/CatalogItem';

@Injectable({
  providedIn: 'root'
})
export class BasketService {

  constructor(private oauthService: OAuthService,
    private http: HttpClient,
    private shoppingErrorHandler: ShoppingErrorHandler) { }

  getUserBasket(userId: string): Observable<HttpResponse<ShoppingBasketItem[]>> {
    if(userId == null)
    {
      console.log('User id must have a value.');
      return new Observable<HttpResponse<ShoppingBasketItem[]>>();
    }

    let url = `${Constants.apiGatewayBaseUrl}/Basket/api/${userId}`;
    let headers = new HttpHeaders()
                        .set('Accept', 'application/json')
                        .set('Authorization', `Bearer ${this.oauthService.getAccessToken()}`);

    return this.http
        .get<ShoppingBasketItem[]>(url, { observe: 'response', headers: headers })
        .pipe(
          retry(3), // retry a failed request up to 3 times
          catchError(this.shoppingErrorHandler.handleError) // then handle the error
        );
  }

  addItemToBasket(item: CatalogItem): Observable<HttpResponse<ShoppingBasketItem>> {
    let url = `${Constants.apiGatewayBaseUrl}/Basket/api/AddBasketItem`;

    let headers = new HttpHeaders()
                        .set('Content-Type', 'application/json')
                        .set('Authorization', `Bearer ${this.oauthService.getAccessToken()}`);

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
}
