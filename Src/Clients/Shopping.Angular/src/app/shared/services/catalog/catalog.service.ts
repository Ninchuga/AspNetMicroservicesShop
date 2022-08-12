import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { Constants } from '../../constants';
import { catchError, retry } from 'rxjs/operators';
import { CatalogItem } from '../../models/CatalogItem';
import { throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CatalogService {

  constructor(private oauthService: OAuthService,
    private http: HttpClient) { }

  public flights: Array<CatalogItem> = [];

  getCatalog() {
    let url = `${Constants.apiGatewayBaseUrl}/Catalog/api`;
    let headers = new HttpHeaders()
                        .set('Accept', 'application/json')
                        .set('Authorization', `Bearer ${this.oauthService.getAccessToken()}`);

    const requestOptions = { headers: headers };
    //let params = new HttpParams().set('from', from).set('to', to);
    return this.http
        .get<CatalogItem[]>(url, requestOptions)
        .pipe(
          retry(3), // retry a failed request up to 3 times
          catchError(this.handleError) // then handle the error
        );
  }

  private handleError(error: HttpErrorResponse) {
    if (error.status === 0) {
      // A client-side or network error occurred. Handle it accordingly.
      console.error('An error occurred:', error.error);
    } else {
      // The backend returned an unsuccessful response code.
      // The response body may contain clues as to what went wrong.
      console.error(
        `Backend returned code ${error.status}, body was: `, error.error);
    }
    // Return an observable with a user-facing error message.
    return throwError(() => new Error('Something bad happened; please try again later.'));
  }
}
