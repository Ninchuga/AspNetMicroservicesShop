import { Component, Inject, LOCALE_ID, OnInit } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { UserOrder } from '../../models/UserOrder';
import { OrderService } from '../../services/order/order.service';
import {formatDate} from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { HttpResponse } from '@angular/common/http';

@Component({
  selector: 'app-orders',
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.css']
})
export class OrdersComponent implements OnInit {

  userOrders: Array<UserOrder> = [];

  constructor(private oauthService: OAuthService,
              private orderService: OrderService,
              @Inject(LOCALE_ID) private locale: string,
              private route: ActivatedRoute) { }

  ngOnInit(): void {
    this.getUserOrders();
  }

  getUserOrders() {
    this.route.data
        .subscribe(data => {
          const response: HttpResponse<UserOrder[]> = data['ordersResponse'];
          this.userOrders = response.body ?? [];
          for(let order of this.userOrders) {
            order.orderDate = formatDate(order.orderDate, 'mediumDate', this.locale);
          }
        });
  }

  orderDetails(order: UserOrder){
    console.log('Order details...');
  }

  displayedColumns = [ 'orderNumber', 'address', 'orderDate',  'price', 'status', 'action' ];
}
