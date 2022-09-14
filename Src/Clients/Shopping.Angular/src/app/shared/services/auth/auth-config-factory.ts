import { OAuthModuleConfig } from "angular-oauth2-oidc";
import { SettingsService } from "../settings/settings.service";

export function authConfigFactory(service: SettingsService): OAuthModuleConfig {
    return {
      resourceServer: {
        allowedUrls: [service.settings.apiGatewayBaseUrl],
        sendAccessToken: true,
      }
    };
  }