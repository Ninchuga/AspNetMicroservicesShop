import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { CatalogItem } from '../../models/CatalogItem';
import { BasketService } from '../../services/basket/basket.service';

@Component({
  selector: 'app-catalog',
  templateUrl: './catalog.component.html',
  styleUrls: ['./catalog.component.css']
})
export class CatalogComponent implements OnInit {

  catalogItems: Array<CatalogItem> = [];
  itemQuantityForm: FormGroup = new FormGroup({});

  constructor(private basketService: BasketService,
              private fb: FormBuilder,
              private route: ActivatedRoute) { }

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
    this.route.data
    .subscribe(data => {
      this.catalogItems = data['catalogItems'];
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
