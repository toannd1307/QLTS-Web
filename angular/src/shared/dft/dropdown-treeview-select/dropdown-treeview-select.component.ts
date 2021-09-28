// tslint:disable
import { DropdownTreeviewSelectI18n } from './dropdown-treeview-select-i18n';
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, ViewChild, ViewEncapsulation } from '@angular/core';
import { isNil } from 'lodash';
import { TreeviewHelper } from './lib/helpers/treeview-helper';
import { TreeviewI18n } from './lib/models/treeview-i18n';
import { TreeviewItem } from './lib/models/treeview-item';
import { TreeviewConfig } from './lib/models/treeview-config';
import { DropdownTreeviewComponent } from './lib/components/dropdown-treeview/dropdown-treeview.component';

@Component({
  selector: 'app-dropdown-treeview-select',
  templateUrl: './dropdown-treeview-select.component.html',
  styleUrls: ['./dropdown-treeview-select.component.scss'],
  // encapsulation: ViewEncapsulation.None,
  providers: [
    { provide: TreeviewI18n, useClass: DropdownTreeviewSelectI18n }
  ]
})
export class DropdownTreeviewSelectComponent implements OnInit, OnChanges {

  @Input() config: TreeviewConfig;
  @Input() items: TreeviewItem[];
  @Input() value: any;
  @Input() disabled: boolean;
  @Output() valueChange = new EventEmitter<any>();
  @ViewChild(DropdownTreeviewComponent, { static: false }) dropdownTreeviewComponent: DropdownTreeviewComponent;
  filterText: string;
  private dropdownTreeviewSelectI18n: DropdownTreeviewSelectI18n;

  constructor(
    public i18n: TreeviewI18n
  ) {
    this.config = TreeviewConfig.create({
      hasAllCheckBox: false,
      hasCollapseExpand: true,
      hasFilter: true,
      maxHeight: 500
    });
    this.dropdownTreeviewSelectI18n = i18n as DropdownTreeviewSelectI18n;
  }
  ngOnInit(): void {
  }

  ngOnChanges(): void {
    this.updateSelectedItem();
  }

  select(item: TreeviewItem): void {
    this.selectItem(item);
  }

  clear() {
    this.value = undefined;
    this.valueChange.emit(undefined);
    this.updateSelectedItem();
  }

  private updateSelectedItem(): void {
    if (!isNil(this.items)) {
      const selectedItem = TreeviewHelper.findItemInList(this.items, this.value);
      this.selectItem(selectedItem);
    }
  }

  private selectItem(item: TreeviewItem): void {
    if (this.dropdownTreeviewSelectI18n.selectedItem !== item) {
      this.dropdownTreeviewSelectI18n.selectedItem = item;
      if (this.dropdownTreeviewComponent) {
        this.dropdownTreeviewComponent.onSelectedChange([item]);
      }

      if (item && this.value !== item.value) {
        this.value = item.value;
        this.valueChange.emit(item.value);
      }
    }
  }
}
