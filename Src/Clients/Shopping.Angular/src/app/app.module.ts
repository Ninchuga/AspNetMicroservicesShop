import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { HttpClientModule } from '@angular/common/http';
import { OAuthModule } from 'angular-oauth2-oidc';
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
import { OrdersComponent } from './shared/components/orders/orders.component';
import { SigninRedirectCallbackComponent } from './shared/components/auth/signin-redirect-callback.component';
import { SignoutRedirectComponent } from './shared/components/auth/signout-redirect.component'
import { NotFoundComponent } from './shared/components/auth/not-found.component';
import { MatSelectModule } from '@angular/material/select';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    NavComponent,
    CatalogComponent,
    BasketComponent,
    OrdersComponent,
    SigninRedirectCallbackComponent,
    SignoutRedirectComponent,
    NotFoundComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    OAuthModule.forRoot({
      resourceServer: {
          allowedUrls: ['http://localhost:5006/Catalog/api', 'http://localhost:5006/Basket/api', 'http://localhost:5005'],
          //allowedUrls: ['http://localhost:4200'],
          sendAccessToken: true // this will enable setting access token in request header for the specified resource url prefixes. This is http interceptor out of the box and http error handling
      }}),
    BrowserAnimationsModule,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatSelectModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
