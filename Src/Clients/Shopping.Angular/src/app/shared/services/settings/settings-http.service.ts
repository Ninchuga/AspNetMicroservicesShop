import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Settings } from 'src/app/settings';
import { SettingsService } from './settings.service';

@Injectable({
  providedIn: 'root'
})
export class SettingsHttpService {

  constructor(private http: HttpClient, private settingsService: SettingsService) { }

  initializeApp(): Promise<any> {

    return new Promise(
        (resolve) => {
            this.http.get('assets/settings.json')
                .subscribe({
                  next: (response: any) => {
                    console.log('loading settings...')
                    console.log(response)
                    this.settingsService.settings = <Settings>response;
                    resolve(response);
                  }
                })
        }
    );
} 
}
