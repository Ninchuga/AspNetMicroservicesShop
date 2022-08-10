import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-signout-redirect-callback',
  template: `<div></div>`
})
export class SignoutRedirectComponent implements OnInit {

  constructor(private _router: Router) { }

  ngOnInit(): void {
    this._router.navigate(['/home'], { replaceUrl: true });
  }

}
