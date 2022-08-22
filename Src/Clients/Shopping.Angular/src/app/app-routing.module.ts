import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './shared/components/home/home.component';
import { CatalogComponent } from './shared/components/catalog/catalog.component';
import { BasketComponent } from './shared/components/basket/basket.component';
import { OrdersComponent } from './shared/components/orders/orders.component';
import { SigninRedirectCallbackComponent } from './shared/components/auth/signin-redirect-callback.component'
import { NotFoundComponent } from './shared/components/auth/not-found.component'
import { SignoutRedirectComponent as SignoutRedirectCallbackComponent } from './shared/components/auth/signout-redirect.component'
import { CheckoutComponent } from './shared/components/checkout/checkout/checkout.component';

const routes: Routes = [
  { path: 'home', component: HomeComponent },
  { path: 'catalog', component: CatalogComponent },
  { path: 'basket', component: BasketComponent },
  { path: 'orders', component: OrdersComponent },
  { path: 'checkout', component: CheckoutComponent },
  { path: 'signin-callback', component: SigninRedirectCallbackComponent }, // TODO: Remove this
  { path: 'signout-callback', component: SignoutRedirectCallbackComponent }, // TODO: Remove this
  { path: 'silent-refresh', redirectTo: "/silent-refresh.html" }, // TODO: Remove this
  { path: '404', component : NotFoundComponent},
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: '**', redirectTo: '/404', pathMatch: 'full'} // unsupported path
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
