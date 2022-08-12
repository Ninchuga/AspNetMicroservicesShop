import { Component, OnInit } from '@angular/core';
import { OAuthErrorEvent, OAuthService } from 'angular-oauth2-oidc';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  claims:any;
  constructor(private oauthService: OAuthService) { 
    // Useful for debugging:
    this.oauthService.events.subscribe(event => {
      if (event instanceof OAuthErrorEvent) {
        console.error('OAuthErrorEvent Object:', event);
      } else {
        console.warn('OAuthEvent Object:', event);
      }
    });
  }

  ngOnInit(): void {
  }

  public login() {
    console.log("Loging in...");
    //this.oauthService.initLoginFlow();
    this.oauthService.initCodeFlow();
  }

  public logout() {
    console.log("Loging out...");
    this.oauthService.logOut();
    //this.oauthService.revokeTokenAndLogout();
  }

  get isUserLoggedIn(){
    let loggedIn = this.oauthService.hasValidAccessToken();
    console.log(`User logged in: ${loggedIn}`);
    return loggedIn;
  }

  get userClaims(){
    this.claims = this.oauthService.getIdentityClaims();
    if(this.claims){
      console.log("Login claims: ", this.claims);
    }

    return this.claims ? this.claims : null;
  }

  get useHashLocationStrategy() {
    return localStorage.getItem('useHashLocationStrategy') === 'true';
  }

  get id_token() {
    let idToken = this.oauthService.getIdToken();
    console.log(`Id token: ${idToken}`);
    return idToken;
  }

  get access_token() {
    let accessToken = this.oauthService.getAccessToken();
    console.log(`Access token: ${accessToken}`);
    return accessToken;
  }
}
