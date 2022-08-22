import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Constants } from '../../constants';
import { catchError, retry } from 'rxjs/operators';
import { CatalogItem } from '../../models/CatalogItem';
import { ShoppingErrorHandler } from 'src/app/errorHandler';

@Injectable({
  providedIn: 'root'
})
export class CatalogService {

  constructor(private http: HttpClient,
              private shoppingErrorHandler: ShoppingErrorHandler) { }

  getCatalog() {
    let url = `${Constants.apiGatewayBaseUrl}/Catalog/api`;
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
