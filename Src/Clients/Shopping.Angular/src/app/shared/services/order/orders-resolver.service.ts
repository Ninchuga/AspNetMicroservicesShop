import { HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { Observable } from 'rxjs';
import { UserOrder } from '../../models/UserOrder';
import { OrderService } from './order.service';

@Injectable({
  providedIn: 'root'
})
export class OrdersResolverService implements Resolve<HttpResponse<UserOrder[]>> {

  constructor(private orderService: OrderService,
              private oauthService: OAuthService) { }
              
  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): HttpResponse<UserOrder[]> | Observable<HttpResponse<UserOrder[]>> | Promise<HttpResponse<UserOrder[]>> {
    const claims = this.oauthService.getIdentityClaims();
    if(!claims){
      console.warn("Couldn't find user claims.");
      return new HttpResponse<UserOrder[]>();
    }

    const userId = claims['sub'];
    if(!userId)
    {
      console.warn("User id must be provided.")
      return new HttpResponse<UserOrder[]>();
    }

     return this.orderService.getUserOrders(userId);
  }
}
