import { InjectionToken } from "@angular/core";

export const APP_CONFIG = new InjectionToken<Settings>('config from settings.json');

export class Settings {
    apiGatewayBaseUrl!: string;
    idpAuthority!: string;
    clientId!: string;
}