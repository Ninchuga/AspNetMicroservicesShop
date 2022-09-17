import { OAuthModuleConfig } from "angular-oauth2-oidc";
import { Settings } from "src/app/settings";

export function authConfigFactory(settings: Settings): OAuthModuleConfig {
  return {
    resourceServer: {
      allowedUrls: [settings.apiGatewayBaseUrl],
      sendAccessToken: true // this will enable setting access token in request header for the specified resource url prefixes. This is http interceptor out of the box and http error handling
    }
  };
}