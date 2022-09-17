import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable, Optional } from "@angular/core";
import { OAuthModuleConfig, OAuthResourceServerErrorHandler, OAuthStorage } from "angular-oauth2-oidc";
import { Observable } from "rxjs";
import { catchError } from 'rxjs/operators';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(private authStorage: OAuthStorage,
                private errorHandler: OAuthResourceServerErrorHandler,
                @Optional() private moduleConfig: OAuthModuleConfig) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        let url = request.url.toLowerCase();

        console.log('Executing auth interceptor...')

        if (!this.moduleConfig) return next.handle(request);
        if (!this.moduleConfig.resourceServer) return next.handle(request);
        if (!this.moduleConfig.resourceServer.allowedUrls) return next.handle(request);
        if (!this.checkUrl(url)) return next.handle(request);

        let sendAccessToken = this.moduleConfig.resourceServer.sendAccessToken;
        if (sendAccessToken) {
            console.log('send access token')
            request = request.clone({
                headers: request.headers.set('Authorization', `Bearer ${this.authStorage.getItem('access_token')}`)
            });
        }

        return next.handle(request).pipe(
            catchError(err => this.errorHandler.handleError(err))
        );
    }

    private checkUrl(url: string): boolean {
        console.log(`checking url ${url}`)
        let found = this.moduleConfig.resourceServer.allowedUrls?.find(u => url.startsWith(u));
        return !!found;
    }
}
