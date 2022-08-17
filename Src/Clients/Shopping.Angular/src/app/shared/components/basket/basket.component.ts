import { Component, OnInit } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { ShoppingBasketItem } from '../../models/ShoppingBasketItem';
import { BasketService } from '../../services/basket/basket.service';

@Component({
  selector: 'app-basket',
  templateUrl: './basket.component.html',
  styleUrls: ['./basket.component.css']
})
export class BasketComponent implements OnInit {

  public userBasketItems: Array<ShoppingBasketItem> = [];

  constructor(private oauthService: OAuthService,
              private basketService: BasketService) { }

  ngOnInit(): void {
    this.getUserBasket();
  }

  getUserBasket() {
    let claims = this.oauthService.getIdentityClaims();
    if(!claims){
      console.log("Couldn't find user claims.");
      return;
    }

    let userId = claims['sub'];
    this.basketService.getUserBasket(userId)
    .subscribe(response => {
      console.log(response);
      //const keys = response.headers.keys();
      //let headers = keys.map(key =>
      //  `${key}: ${response.headers.get(key)}`);
        this.userBasketItems = response.body ?? [];
    });
  }

  deleteBasketItem(productId: string) {
    console.log(`Delete product id ${productId} from basket`);
  }

  displayedColumns = [ 'productName', 'quantity', 'price',  'discount', 'priceWithDiscount', 'action' ];
}
