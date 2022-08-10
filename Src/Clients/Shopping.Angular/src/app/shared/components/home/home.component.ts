import { Component, OnInit } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  constructor(private oauthService: OAuthService) { }

  ngOnInit(): void {
    // let token = this.oauthService.getAccessToken();
    // console.log(`home comp token: ${token}`);

    // let claims = this.oauthService.getIdentityClaims();
    // console.log(`home comp claims: ${claims}`);
  }

  // get useHashLocationStrategy() {
  //   return localStorage.getItem('useHashLocationStrategy') === 'true';
  // }

  // get id_token() {
  //   return this.oauthService.getIdToken();
  // }

  // get has_id_token() {
  //   return this.oauthService.hasValidIdToken();
  // }

  // get access_token() {
  //   return this.oauthService.getAccessToken();
  // }

  // get has_access_token() {
  //   return this.oauthService.hasValidAccessToken();
  // }

  // get id_token_expiration() {
  //   return this.oauthService.getIdTokenExpiration();
  // }

  // get access_token_expiration() {
  //   return this.oauthService.getAccessTokenExpiration();
  // }
}
