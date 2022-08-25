import { HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { Observable } from 'rxjs';
import { ShoppingBasket } from '../../models/ShoppingBasket';
import { BasketService } from './basket.service';

@Injectable({
  providedIn: 'root'
})
export class BasketResolverService implements Resolve<HttpResponse<ShoppingBasket>>  {

  constructor(private basketService: BasketService,
              private oauthService: OAuthService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): HttpResponse<ShoppingBasket> | Observable<HttpResponse<ShoppingBasket>> | Promise<HttpResponse<ShoppingBasket>> {
    const claims = this.oauthService.getIdentityClaims();
    if(!claims){
      console.warn("Couldn't find user claims.");
      return new HttpResponse();
    }

    const userId = claims['sub'];
    if(!userId)
    {
      console.warn("User id must be provided.")
      return new HttpResponse();
    }

    return this.basketService.getUserBasket(userId);
  }
}
