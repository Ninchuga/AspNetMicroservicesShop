import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './shared/components/home/home.component';
import { CatalogComponent } from './shared/components/catalog/catalog.component';
import { BasketComponent } from './shared/components/basket/basket.component';
import { NotFoundComponent } from './shared/components/auth/not-found.component'
import { CheckoutComponent } from './shared/components/checkout/checkout/checkout.component';
import { AuthGuard } from './shared/services/auth/auth.guard';
import { CatalogResolverService } from './shared/services/catalog/catalog-resolver.service';
import { BasketResolverService } from './shared/services/basket/basket-resolver.service';

const routes: Routes = [
  { path: 'home', component: HomeComponent },
  { path: 'catalog', component: CatalogComponent, resolve: { catalogItems: CatalogResolverService } },
  { path: 'basket', component: BasketComponent, canActivate: [AuthGuard], resolve: { basketResponse: BasketResolverService } },
  { path: 'orders', canActivate: [AuthGuard], loadChildren: () => import('./shared/components/orders/orders.module').then(m => m.OrdersModule) }, // lazy loaded component
  { path: 'checkout', component: CheckoutComponent, canActivate: [AuthGuard] },
  { path: '404', component : NotFoundComponent},
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: '**', redirectTo: '/404', pathMatch: 'full'} // unsupported path
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
