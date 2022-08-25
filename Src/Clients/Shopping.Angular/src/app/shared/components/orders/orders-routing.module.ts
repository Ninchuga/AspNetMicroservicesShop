import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { OrdersResolverService } from '../../services/order/orders-resolver.service';
import { OrdersComponent } from './orders.component';

const routes: Routes = [
  { path: '', component: OrdersComponent, resolve:  { ordersResponse: OrdersResolverService } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class OrdersRoutingModule { }
