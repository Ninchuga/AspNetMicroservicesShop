import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OAuthService } from 'angular-oauth2-oidc';
import { OrderItem } from 'src/app/shared/models/OrderItem';
import { ShoppingBasket } from 'src/app/shared/models/ShoppingBasket';
import { ShoppingBasketItem } from 'src/app/shared/models/ShoppingBasketItem';
import { UserOrder } from 'src/app/shared/models/UserOrder';
import { BasketService } from 'src/app/shared/services/basket/basket.service';

@Component({
  selector: 'app-checkout',
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.css']
})
export class CheckoutComponent implements OnInit {

  checkoutForm: FormGroup = new FormGroup({});
  private userBasket: ShoppingBasket = { items: [], totalPrice: 0 };

  constructor(private fb: FormBuilder,
              private oauthService: OAuthService,
              private basketService: BasketService) { }

  ngOnInit(): void {
    this.configureCheckoutForm();
  }

  submit(data){
    if(!this.checkoutForm.valid)
    {
      console.warn('Checkout form is not valid');
      return;
    }

    const claims = this.oauthService.getIdentityClaims();
    if(!claims){
      console.error('Couldn\'t find user claims.');
      return;
    }
    
    const userId = claims['sub'];
    if(!userId)
    {
      console.error('User id must be provided.');
      return;
    }

    this.basketService.getUserBasket(userId)
      .subscribe(response => {
        console.log(response);
        this.userBasket = response.body ?? this.userBasket;
        if(!this.userBasket || !this.userBasket.items)
        {
          console.error('User basket is not valid');
          return;
        }

        const orderItems: OrderItem[] = this.mapToOrderItems(this.userBasket.items);
        const userOrder: UserOrder = this.buildUserOrder(data, userId, claims['preferred_username'], orderItems);
        // TODO: send userOrder request to order service
      });
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

  private buildUserOrder(data: any, userId: string, userName: string, orderItems: OrderItem[]): UserOrder {
    return {
      userId: userId,
      userName: userName,
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
      totalPrice: this.userBasket.totalPrice,
      orderItems: orderItems
    } as UserOrder;
  }

  private configureCheckoutForm() {
    this.checkoutForm = this.fb.group({
      firstName: [null, Validators.required],
      lastName: [null, Validators.required],
      street: [null, Validators.required],
      city: [null, Validators.required],
      country: [null, Validators.required],
      email: [null, Validators.required],
      cardName: [null, Validators.required],
      cardNumber: [null, Validators.required],
      cardExpiration: [null, Validators.required],
      cvv: [null, Validators.required],
    });
  }
}
