import { Component, OnInit } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { UserOrder } from '../../models/UserOrder';
import { OrderService } from '../../services/order/order.service';

@Component({
  selector: 'app-orders',
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.css']
})
export class OrdersComponent implements OnInit {

  userOrders: Array<UserOrder> = [];

  constructor(private oauthService: OAuthService,
              private orderService: OrderService) { }

  ngOnInit(): void {
    this.getUserOrders();
  }

  getUserOrders() {
    const claims = this.oauthService.getIdentityClaims();
    if(!claims){
      console.warn("Couldn't find user claims.");
      return;
    }

    const userId = claims['sub'];
    if(!userId)
    {
      console.warn("User id must be provided.")
      return;
    }

    this.orderService.getUserOrders(userId)
    .subscribe(response => {
      console.log(response);
      this.userOrders = response.body ?? this.userOrders;
    });
  }
}
