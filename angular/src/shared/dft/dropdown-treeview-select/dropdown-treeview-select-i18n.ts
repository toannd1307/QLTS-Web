import { DefaultTreeviewI18n } from './lib/models/treeview-i18n';
import { TreeviewItem, TreeviewSelection } from './lib/models/treeview-item';
// tslint:disable
import { Injectable } from '@angular/core';

@Injectable()
export class DropdownTreeviewSelectI18n extends DefaultTreeviewI18n {
  private internalSelectedItem: TreeviewItem;

  set selectedItem(value: TreeviewItem) {
    this.internalSelectedItem = value;
  }

  get selectedItem(): TreeviewItem {
    return this.internalSelectedItem;
  }

  getText(selection: TreeviewSelection): string {
    return this.internalSelectedItem ? this.internalSelectedItem.text : 'Chọn';
  }
}
