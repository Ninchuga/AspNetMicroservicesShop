import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { APP_CONFIG, Settings } from './app/settings';
import { environment } from './environments/environment';


fetch('/assets/settings.json')
  .then(config => config.json())
  .then((settings: Settings) => {

    if (environment.production) {
      enableProdMode();
    }

    platformBrowserDynamic( [{ provide: APP_CONFIG, useValue: settings }])
    .bootstrapModule(AppModule)
      .catch(err => console.error(err));
  });
