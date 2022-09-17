import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { catchError, retry } from 'rxjs/operators';
import { CatalogItem } from '../../models/CatalogItem';
import { ShoppingErrorHandler } from 'src/app/errorHandler';
import { SettingsService } from '../settings/settings.service';
import { APP_CONFIG, Settings } from 'src/app/settings';

@Injectable({
  providedIn: 'root'
})
export class CatalogService {
  globalSettings: Settings;

  constructor(private http: HttpClient,
              private shoppingErrorHandler: ShoppingErrorHandler,
              private settingsService: SettingsService,
              @Inject(APP_CONFIG)settings: Settings) 
              { 
                this.globalSettings = settings;
              }

  getCatalog() {
    console.log('get catalog ...');
    let url = `${this.globalSettings.apiGatewayBaseUrl}/Catalog/api`;
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
