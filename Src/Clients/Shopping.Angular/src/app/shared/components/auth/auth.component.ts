import { Component, OnInit } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { Router } from '@angular/router';
import { authCodeFlowConfig } from 'src/app/config/authCodeFlowConfig';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css']
})
export class AuthComponent implements OnInit {

  claims:any;

  constructor(
    private oauthService: OAuthService,
    private router: Router) { }

  ngOnInit(): void {
    //this.configAuth();
  }

  // configAuth(){
  //   console.log("Configuring auth code flow...")
  //   this.oauthService.configure(authCodeFlowConfig);
  //   this.oauthService.loadDiscoveryDocumentAndTryLogin();
  //   //this.oauthService.loadDiscoveryDocumentAndLogin();
  // }

  // public login() {
  //   console.log("Login...")
  //   this.oauthService.initLoginFlow();
  //   //this.oauthService.initCodeFlow();
  //   //this.oauthService.setupAutomaticSilentRefresh();
  // }

  // public logout() {
  //     console.log("logging out...")
  //     this.oauthService.logOut();
  // }

  // public isLoggedIn(): boolean {
  //   let loggedIn = this.oauthService.hasValidAccessToken();
  //   return loggedIn;
  // }

  // get token(){
  //   this.claims = this.oauthService.getIdentityClaims();
  //   if(this.claims){
  //     console.log("Login claims: ", this.claims);
  //     this.router.navigateByUrl('home');
  //   }

  //   return this.claims ? this.claims : null;
  // }
}
