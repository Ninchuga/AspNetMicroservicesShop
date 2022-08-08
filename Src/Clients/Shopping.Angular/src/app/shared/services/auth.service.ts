import { Injectable } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(private oauthService:OAuthService) { 
       
    this.oauthService.loadDiscoveryDocumentAndTryLogin().then(
      (isLoggedIn) => {

        if (isLoggedIn) {
          this.oauthService.setupAutomaticSilentRefresh();
        }
      },
      (error) => {
        console.log({ error });
        if (error.status === 400) {
          location.reload();
        }
      }
    );
  }
}
