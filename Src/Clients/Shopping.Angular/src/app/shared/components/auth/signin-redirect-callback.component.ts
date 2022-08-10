import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';

@Component({
  selector: 'app-signin-redirect-callback',
  template: `<div></div>`
})
export class SigninRedirectCallbackComponent implements OnInit {

  constructor(private _oauthService: OAuthService, private _router: Router) { }

  ngOnInit(): void {
    let token = this._oauthService.getAccessToken();
    console.log(`sign-in-redirect token: ${token}`);

    let claims = this._oauthService.getIdentityClaims();
    console.log(`sign-in-redirect claims: ${claims}`);

    // Token will only work if we stay on the page
    // If we redirect somewhere else we will loose all the data
    //this._router.navigate(['/'], { replaceUrl: true });
  }

  get access_token () {
    let token = this._oauthService.getAccessToken();
    console.log(`token: ${token}`);
    return token;
  }

}
