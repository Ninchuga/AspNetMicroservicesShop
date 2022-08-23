import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OAuthService } from 'angular-oauth2-oidc';
import { CheckoutData } from 'src/app/shared/models/CheckoutData';
import { OrderService } from 'src/app/shared/services/order/order.service';

@Component({
  selector: 'app-checkout',
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.css']
})
export class CheckoutComponent implements OnInit {

  checkoutForm: FormGroup = new FormGroup({});

  constructor(private fb: FormBuilder,
              private oauthService: OAuthService,
              private orderService: OrderService) { }

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

    const userName = claims['preferred_username'];
    const checkoutData = this.buildCheckoutData(data, userId, userName);

    this.orderService.placeOrder(checkoutData)
      .subscribe(response => {
        console.log(response);
      });
  }

  private buildCheckoutData(data: any, userId: string, userName: string): CheckoutData {
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
    } as CheckoutData;
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
