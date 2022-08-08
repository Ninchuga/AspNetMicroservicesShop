import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './shared/components/home/home.component';
import { CatalogComponent } from './shared/components/catalog/catalog.component';
import { BasketComponent } from './shared/components/basket/basket.component';
import { OrdersComponent } from './shared/components/orders/orders.component';
import { AuthComponent } from './shared/components/auth/auth.component'

const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'home', component: HomeComponent },
  { path: 'catalog', component: CatalogComponent },
  { path: 'basket', component: BasketComponent },
  { path: 'orders', component: OrdersComponent },
  { path: 'login', component: AuthComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
