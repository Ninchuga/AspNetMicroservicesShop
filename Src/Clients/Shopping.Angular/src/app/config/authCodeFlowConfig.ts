import { AuthConfig } from 'angular-oauth2-oidc';
import { Constants } from '../shared/constants';

export const authCodeFlowConfig: AuthConfig = {
    // Url of the Identity Provider
    issuer: Constants.idpAuthority,
  
    // URL of the SPA to redirect the user to after login
    //redirectUri: window.location.origin,
    //redirectUri: `${window.location.origin}/signin-callback`,
    //redirectUri: `${window.location.origin}/index.html`,
    redirectUri: `${window.location.origin}/home`,
  
    // The SPA's id. The SPA is registerd with this id at the auth-server
    clientId: Constants.clientId,

    //postLogoutRedirectUri: `${window.location.origin}/signout-callback`,
    postLogoutRedirectUri: `${window.location.origin}/home`,

    logoutUrl: `${window.location.origin}/home`,
  
    // Just needed if your auth server demands a secret. In general, this
    // is a sign that the auth server is not configured with SPAs in mind
    // and it might not enforce further best practices vital for security
    // such applications.
    // dummyClientSecret: 'secret',
    responseType: 'code',
  
    // set the scope for the permissions the client should request
    // The first four are defined by OIDC.
    // Important: Request offline_access to get a refresh token
    // The api scope is a usecase specific one
    scope: 'openid profile roles address offline_access',
    //scope: 'openid profile email offline_access api', // for azure
  
    // This is needed for silent refresh (refreshing tokens w/o a refresh_token)
    // **AND** for logging in with a popup
    silentRefreshRedirectUri: `${window.location.origin}/silent-refresh.html`,
  
    useSilentRefresh: true,
  
    sessionChecksEnabled: false,
  
    clearHashAfterLogin: true,
  
    showDebugInformation: true
  };