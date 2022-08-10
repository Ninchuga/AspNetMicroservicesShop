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
    //this.oauthService.initLoginFlow('/some-state;p1=1;p2=2?p3=3&p4=4');
  }

  public logout() {
    console.log("Loging out...");
    this.oauthService.logOut();
    //this.oauthService.revokeTokenAndLogout();
  }

  get token(){
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
    return this.oauthService.getIdToken();
  }

  get access_token() {
    return this.oauthService.getAccessToken();
  }

  get id_token_expiration() {
    return this.oauthService.getIdTokenExpiration();
  }

  get access_token_expiration() {
    return this.oauthService.getAccessTokenExpiration();
  }

  loadUserProfile(): void {
    //this.oauthService.loadUserProfile().then((up) => (this.userProfile = up));
  }

  startAutomaticRefresh(): void {
    this.oauthService.setupAutomaticSilentRefresh();
  }

  stopAutomaticRefresh(): void {
    this.oauthService.stopAutomaticRefresh();
  }
}
