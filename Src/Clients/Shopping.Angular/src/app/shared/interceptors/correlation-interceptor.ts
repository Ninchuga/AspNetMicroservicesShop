import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { v4 as uuid } from 'uuid';

@Injectable()
export class CorrelationInterceptor implements HttpInterceptor {
    constructor() { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const correlationId = uuid();
        request = request.clone({
            headers: request.headers.set('X-Correlation-ID', correlationId)
        });

        return next.handle(request);
    }
}