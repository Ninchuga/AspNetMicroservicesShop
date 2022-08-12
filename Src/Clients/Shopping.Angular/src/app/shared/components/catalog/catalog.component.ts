import { Component, OnInit } from '@angular/core';
import { CatalogItem } from '../../models/CatalogItem';
import { CatalogService } from '../../services/catalog/catalog.service';

@Component({
  selector: 'app-catalog',
  templateUrl: './catalog.component.html',
  styleUrls: ['./catalog.component.css']
})
export class CatalogComponent implements OnInit {

  public catalogItems: Array<CatalogItem> = [];

  constructor(private catalogService: CatalogService) { }

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
  }

  displayedColumns = [ 'name', 'category', 'description', 'price', 'quantity', 'summary', 'action' ];
}
