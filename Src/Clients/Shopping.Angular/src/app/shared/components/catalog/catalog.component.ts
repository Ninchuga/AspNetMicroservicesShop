import { HttpResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { CatalogItem } from '../../models/CatalogItem';
import { BasketService } from '../../services/basket/basket.service';
import { CatalogService } from '../../services/catalog/catalog.service';

@Component({
  selector: 'app-catalog',
  templateUrl: './catalog.component.html',
  styleUrls: ['./catalog.component.css']
})
export class CatalogComponent implements OnInit {

  public catalogItems: Array<CatalogItem> = [];

  constructor(private catalogService: CatalogService,
              private basketService: BasketService) { }

  ngOnInit(): void {
     this.getCatalog();
  }

  getCatalog() {
    this.catalogService.getCatalog()
    .subscribe(response => {
      this.catalogItems = response;
    });
  }

  addItemToBasket(item: CatalogItem) {
    console.log(`adding item to the basket. Item: ${item}`);
    this.basketService.addItemToBasket(item)
    .subscribe(response => {
      console.log(response);
      //const keys = response.headers.keys();
      //this.headers = keys.map(key =>
      //  `${key}: ${response.headers.get(key)}`);
      });
  }

  displayedColumns = [ 'name', 'category', 'description', 'price', 'quantity', 'summary', 'action' ];
  itemQuantities = [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ];
}
