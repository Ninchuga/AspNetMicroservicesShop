import { AuthConfig } from 'angular-oauth2-oidc';
import { Constants } from '../shared/constants';

export const authCodeFlowConfig: AuthConfig = {
    // Url of the Identity Provider
    issuer: Constants.idpAuthority,
  
    // URL of the SPA to redirect the user to after login
    redirectUri: `${window.location.origin}/home`,
  
    // The SPA's id. The SPA is registerd with this id at the auth-server
    clientId: Constants.clientId,

    //postLogoutRedirectUri: `${window.location.origin}/signout-callback`,
    postLogoutRedirectUri: `${window.location.origin}/home`,

    responseType: 'code',
  
    // set the scope for the permissions the client should request
    scope: 'openid profile roles address offline_access shoppinggateway.fullaccess shoppingaggregator.fullaccess',
    
    //scope: 'openid profile email offline_access api', // for azure
  
    // This is needed for silent refresh (refreshing tokens w/o a refresh_token)
    // **AND** for logging in with a popup
    //silentRefreshRedirectUri: `${window.location.origin}/silent-refresh.html`,
  
    //useSilentRefresh: true,
  
    sessionChecksEnabled: false,
  
    clearHashAfterLogin: true,
  
    showDebugInformation: true
  };
