import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, retry } from 'rxjs/operators';
import { CatalogItem } from '../../models/CatalogItem';
import { ShoppingErrorHandler } from 'src/app/errorHandler';
import { SettingsService } from '../settings/settings.service';

@Injectable({
  providedIn: 'root'
})
export class CatalogService {

  constructor(private http: HttpClient,
              private shoppingErrorHandler: ShoppingErrorHandler,
              private settingsService: SettingsService) { }

  getCatalog() {
    let url = `${this.settingsService.settings.apiGatewayBaseUrl}/Catalog/api`;
    //let url = '/Catalog/api';
    let headers = new HttpHeaders()
                        .set('Accept', 'application/json');

    const requestOptions = { headers: headers };
    
    return this.http
        .get<CatalogItem[]>(url, requestOptions)
        .pipe(
          retry(3), // retry a failed request up to 3 times
          catchError(this.shoppingErrorHandler.handleError) // then handle the error
        );
  }
}
