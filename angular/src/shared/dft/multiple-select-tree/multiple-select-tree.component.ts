import { Component, Injector, Input, OnInit, ViewChild, OnChanges, SimpleChanges, EventEmitter, Output, HostListener, ElementRef } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { FlatTreeSelectDto, LookupTableServiceProxy } from '@shared/service-proxies/service-proxies';
import { timeStamp } from 'console';
import { truncate } from 'lodash';
import { PermissionTreeEditModel } from '../components/permission-tree-edit.model';
import { PermissionTreeComponent } from '../components/permission-tree.component';
import { isNil } from 'lodash';

@Component({
  selector: 'app-multiple-select-tree',
  templateUrl: './multiple-select-tree.component.html',
  styleUrls: ['./multiple-select-tree.component.scss']
})
export class MultipleSelectTreeComponent extends AppComponentBase implements OnChanges {

  @Output() onSelect = new EventEmitter<number[]>();
  @ViewChild('permissionTree', { static: true }) permissionTree: PermissionTreeComponent;
  @ViewChild('treeItem', { static: true }) treeItem: ElementRef;
  @ViewChild('btn', { static: true }) btn: ElementRef;
  @Input() dataEdit: PermissionTreeEditModel;
  show = true;
  selectedValue = 'Chọn';
  constructor(
    injector: Injector,
  ) {
    super(injector);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.show && !this.btn.nativeElement.contains(event.target)) {
      this.show = !this._isEventFromToggle(event);
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.dataEdit.currentValue !== changes.dataEdit.previousValue) {
      if (this.dataEdit.data[0].parentId != null) {
        this.dataEdit.data[0].parentId = null;
      }
      this.permissionTree.editData = changes.dataEdit.currentValue;
    }
  }

  onSelectedChange(event: FlatTreeSelectDto[]) {
    this.selectedValue = event.length > 0 ? event.map(e => e.displayName).join() : 'Chọn';
    this.onSelect.emit(event.map(e => e.id));
  }

  private _isEventFromToggle(event: MouseEvent): boolean {
    return !isNil(this.treeItem) && this.treeItem.nativeElement.contains(event.target);
  }
}
