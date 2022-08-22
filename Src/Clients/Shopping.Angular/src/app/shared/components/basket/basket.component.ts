import { Component, OnInit } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { ShoppingBasket } from '../../models/ShoppingBasket';
import { BasketService } from '../../services/basket/basket.service';

@Component({
  selector: 'app-basket',
  templateUrl: './basket.component.html',
  styleUrls: ['./basket.component.css']
})
export class BasketComponent implements OnInit {

  public userBasket: ShoppingBasket = { items: [], totalPrice: 0 };

  constructor(private oauthService: OAuthService,
              private basketService: BasketService) { }

  ngOnInit(): void {
    this.getUserBasket();
  }

  getUserBasket() {
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

    this.basketService.getUserBasket(userId)
    .subscribe(response => {
      console.log(response);
      this.userBasket = response.body ?? this.userBasket;
    });
  }

  deleteBasketItem(productId: string) {
    console.log(`Delete product id ${productId} from user basket`);

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

    this.basketService.deleteBasketItem(productId)
      .subscribe(response => {
        console.log(response);
        this.userBasket = response.body ?? this.userBasket;
      });
  }

  displayedColumns = [ 'productName', 'quantity', 'price',  'discount', 'priceWithDiscount', 'action' ];
}
