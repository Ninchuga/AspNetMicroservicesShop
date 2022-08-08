import { AuthConfig } from 'angular-oauth2-oidc';
import { Constants } from '../shared/constants';

export const authCodeFlowConfig: AuthConfig = {
    // Url of the Identity Provider
    issuer: Constants.idpAuthority,
  
    // URL of the SPA to redirect the user to after login
    //redirectUri: window.location.origin,
    redirectUri: window.location.origin + '/signin-callback',
  
    // The SPA's id. The SPA is registerd with this id at the auth-server
    // clientId: 'server.code',
    clientId: Constants.clientId,

    postLogoutRedirectUri: window.location.origin + '/signout-callback',
  
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
  
    // This is needed for silent refresh (refreshing tokens w/o a refresh_token)
    // **AND** for logging in with a popup
    // silentRefreshRedirectUri: `${window.location.origin}/silent-refresh.html`,
  
    // useSilentRefresh: true,
  
    // sessionChecksEnabled: false,
  
    // clearHashAfterLogin: true,
  
    //showDebugInformation: true
  };