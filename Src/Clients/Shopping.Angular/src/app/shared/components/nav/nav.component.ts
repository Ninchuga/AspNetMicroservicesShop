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
    console.log("Login...");
    this.oauthService.initLoginFlow();
    //this.oauthService.initCodeFlow();
  }

  public logout() {
    console.log("Logout...");
    this.oauthService.logOut();
  }

  get token(){
    this.claims = this.oauthService.getIdentityClaims();
    if(this.claims){
      console.log("Login claims: ", this.claims);
    }

    return this.claims ? this.claims : null;
  }
}
