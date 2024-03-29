import { Component, Inject } from '@angular/core';
import { AuthConfig, OAuthService } from 'angular-oauth2-oidc';
import { filter } from 'rxjs';
import { APP_CONFIG, Settings } from './settings';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Shopping.Angular';
  globalSettings: Settings;

  constructor(private oauthService: OAuthService,
              @Inject(APP_CONFIG)settings: Settings) 
  {
    this.globalSettings = settings;
    this.configureAuth();
    this.loadUserProfile();
  }

  private loadUserProfile(){
    // Automatically load user profile
    this.oauthService.events
      .pipe(filter((e) => e.type === 'token_received'))
      .subscribe((_) => {
        console.debug('state', this.oauthService.state);
        this.oauthService.loadUserProfile();

        const scopes = this.oauthService.getGrantedScopes();
        console.debug('scopes', scopes);
      });
  }

  configureAuth(){
    console.log('test log auth component')
    console.log("Configuring auth code flow...")

    const authCodeFlowConfig: AuthConfig = {
      // Url of the Identity Provider
      issuer: this.globalSettings.idpAuthority,
    
      // URL of the SPA to redirect the user to after login
      redirectUri: `${window.location.origin}/home`,
    
      // The SPA's id. The SPA is registerd with this id at the auth-server
      clientId: this.globalSettings.clientId,
  
      //postLogoutRedirectUri: `${window.location.origin}/signout-callback`,
      postLogoutRedirectUri: `${window.location.origin}/home`,
  
      responseType: 'code',
    
      // set the scope for the permissions the client should request
      scope: 'openid profile roles address offline_access shoppinggateway.fullaccess shoppingaggregator.fullaccess',
      //scope: 'openid profile email offline_access api', // for azure
    
      sessionChecksEnabled: false,
      clearHashAfterLogin: true,
      showDebugInformation: true
    };

    this.oauthService.configure(authCodeFlowConfig);
    this.oauthService.loadDiscoveryDocumentAndTryLogin().then(
      (success) => {
        if (success) {
          // do something
          //this.oauthService.setupAutomaticSilentRefresh();
        }
      },
      (error) => {
        console.log({ error });
        if (error.status === 400) {
          location.reload();
        }
      });

    this.oauthService.setupAutomaticSilentRefresh();
    //sessionStorage.setItem('flow', 'code'); // not needed. used as an example
  }
}
