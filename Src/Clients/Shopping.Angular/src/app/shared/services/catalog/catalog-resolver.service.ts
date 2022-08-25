import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { CatalogItem } from '../../models/CatalogItem';
import { CatalogService } from './catalog.service';

@Injectable({
  providedIn: 'root'
})
export class CatalogResolverService implements Resolve<CatalogItem[]> {

  constructor(private catalogService: CatalogService) { }

  resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): CatalogItem[] | Observable<CatalogItem[]> | Promise<CatalogItem[]> {
    return this.catalogService.getCatalog();
  }
}
