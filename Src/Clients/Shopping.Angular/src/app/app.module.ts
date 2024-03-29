import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { OAuthModule, OAuthModuleConfig } from 'angular-oauth2-oidc';
import { HomeComponent } from './shared/components/home/home.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatButtonModule } from '@angular/material/button'
import { MatToolbarModule } from '@angular/material/toolbar'
import { MatSidenavModule } from '@angular/material/sidenav'
import { MatListModule } from '@angular/material/list'
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { NavComponent } from './shared/components/nav/nav.component';
import { CatalogComponent } from './shared/components/catalog/catalog.component';
import { BasketComponent } from './shared/components/basket/basket.component';
import { NotFoundComponent } from './shared/components/auth/not-found.component';
import { MatSelectModule } from '@angular/material/select';
import { CheckoutComponent } from './shared/components/checkout/checkout/checkout.component';
import { ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { CorrelationInterceptor } from './shared/interceptors/correlation-interceptor';
import { authConfigFactory } from './shared/services/auth/auth-config-factory';
import { APP_CONFIG, Settings } from './settings';

// initialize configuration
// export function app_Init(settingsHttpService: SettingsHttpService) {
//   return () => settingsHttpService.initializeApp();
// }

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    NavComponent,
    CatalogComponent,
    BasketComponent,
    NotFoundComponent,
    CheckoutComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    OAuthModule.forRoot(),
    BrowserAnimationsModule,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatSelectModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressBarModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: CorrelationInterceptor, multi: true },
    //{ provide: APP_INITIALIZER, useFactory: app_Init, deps: [SettingsHttpService], multi: true }, // application should load the settings provided in the settings.json file on startup
    Settings,
    {
      provide: OAuthModuleConfig,
      useFactory: authConfigFactory, // load allowed urls from settings to OAuthModule and enable default auth interceptor
      deps: [APP_CONFIG],
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
