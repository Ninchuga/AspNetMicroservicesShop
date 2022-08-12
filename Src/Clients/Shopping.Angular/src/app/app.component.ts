import { Component } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { filter } from 'rxjs';
import { authCodeFlowConfig } from 'src/app/config/authCodeFlowConfig';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Shopping.Angular';

  constructor(private oauthService: OAuthService) {
    this.configureAuth();
    
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
    console.log("Configuring auth code flow...")
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
