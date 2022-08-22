import { HttpResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
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
  itemQuantityForm: FormGroup = new FormGroup({});

  constructor(private catalogService: CatalogService,
              private basketService: BasketService,
              private fb: FormBuilder) { }

  ngOnInit(): void {
    this.configureItemQuantityForm();
    this.getCatalog();
  }

  private configureItemQuantityForm() {
    this.itemQuantityForm = this.fb.group({
			itemQuantity: [null, Validators.required]
		});

    const deafultQuantity = 1;
    this.itemQuantityForm?.get('itemQuantity')?.setValue(deafultQuantity);
  }

  getCatalog() {
    this.catalogService.getCatalog()
    .subscribe(response => {
      this.catalogItems = response;
    });
  }

  addItemToBasket(item: CatalogItem) {
    const selectedItemQuantity = this.itemQuantityForm?.get('itemQuantity')?.value;
    if(!selectedItemQuantity)
    {
      console.error('item quantity is missing');
      return;
    }

    item.quantity = selectedItemQuantity;
    this.basketService.addItemToBasket(item)
    .subscribe(response => {
      console.log(response);
    });
  }

  displayedColumns = [ 'name', 'category', 'description', 'price', 'quantity', 'summary', 'action' ];
  itemQuantities = [ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 ];
}
