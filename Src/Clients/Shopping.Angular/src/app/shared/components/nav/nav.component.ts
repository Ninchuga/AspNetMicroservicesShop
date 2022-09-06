import { Component, OnInit } from '@angular/core';
import { OAuthErrorEvent, OAuthService } from 'angular-oauth2-oidc';
import { Event, NavigationCancel, NavigationEnd, NavigationError, NavigationStart, Router } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  claims:any;
  loading: boolean = false;

  constructor(private oauthService: OAuthService,
              private router: Router) { 
    this.subscribeToAuthEvents();
    this.subscribeToRoutingEvents();
  }

  ngOnInit(): void {
  }

  private subscribeToRoutingEvents(){
    this.router.events.subscribe((event: Event) => {
      switch (true) {
        case event instanceof NavigationStart: {
          this.loading = true;
          break;
        }

        case event instanceof NavigationEnd:
        case event instanceof NavigationCancel:
        case event instanceof NavigationError: {
          this.loading = false;
          break;
        }
        default: {
          break;
        }
      }
    });
  }

  private subscribeToAuthEvents(){
    // Useful for debugging:
    this.oauthService.events.subscribe(event => {
      if (event instanceof OAuthErrorEvent) {
        console.error('OAuthErrorEvent Object:', event);
      } else {
        console.warn('OAuthEvent Object:', event);
      }
    });
  }

  public login() {
    this.oauthService.initCodeFlow();
  }

  public logout() {
    this.oauthService.logOut();
  }

  get isUserLoggedIn(){
    let loggedIn = this.oauthService.hasValidAccessToken();
    return loggedIn;
  }

  get userClaims(){
    this.claims = this.oauthService.getIdentityClaims();
    
    return this.claims ? this.claims : null;
  }
}
